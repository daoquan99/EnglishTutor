using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Application.DateTime;

namespace EnglishTutor.Identity.Application.Commands.Refresh;

/// <summary>
/// Handles <see cref="RefreshCommand"/>: validates the supplied refresh
/// token, detects reuse, rotates to a new refresh token, and issues a new
/// access token.
/// <para>
/// Persistence-agnostic: depends only on Application abstractions
/// (<c>IUserSessionRepository</c>, <c>IUserRepository</c>,
/// <c>IRoleRepository</c>, <c>IIdentityUnitOfWork</c>,
/// <c>IJwtTokenService</c>, and the refresh-token Application
/// abstractions). It does NOT reference <c>the DbContext abstraction</c> or any
/// other Infrastructure type.
/// </para>
/// </summary>
public sealed class RefreshCommandHandler : ICommandHandler<RefreshCommand, RefreshResult>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenLifetime;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IIdentitySecurityEventService _securityEventService;
    private readonly IDateTimeProvider _clock;

    public RefreshCommandHandler(
        IUserSessionRepository userSessionRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IIdentitySecurityEventService securityEventService,
        IDateTimeProvider clock)
    {
        _userSessionRepository = userSessionRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _refreshTokenLifetime = refreshTokenLifetime;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _securityEventService = securityEventService;
        _clock = clock;
    }

    public async Task<Result<RefreshResult>> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHash.FromHex(_refreshTokenHasher.Hash(request.RefreshToken));
        var snapshot = await _userSessionRepository.FindByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (snapshot is null)
        {
            await _securityEventService.TrackRefreshFailedAsync(
                sessionId: null,
                userId: null,
                refreshTokenFamilyId: null,
                refreshTokenId: null,
                reasonCode: "token_not_found",
                ipAddress: request.IpAddress,
                cancellationToken);

            // Flush the staged security-event outbox row (H-07). No Identity
            // state is mutated in this branch; SaveChanges persists only the
            // outbox message so the failed-refresh event is durable.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RefreshFailureResults.InvalidRefreshToken();
        }

        var session = snapshot.Session;
        var family = snapshot.Family;
        var existing = snapshot.ActiveToken;

        if (session.RevokedAtUtc is not null || family.IsRevoked || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= _clock.UtcNow)
        {
            string reason = "invalid_token";
            if (existing.ExpiresAtUtc <= _clock.UtcNow) reason = "token_expired";
            else if (existing.RevokedAtUtc is not null) reason = "token_revoked";
            else if (family.IsRevoked) reason = "family_revoked";
            else if (session.RevokedAtUtc is not null) reason = "session_revoked";

            await _securityEventService.TrackRefreshFailedAsync(
                sessionId: session.Id,
                userId: existing.UserId,
                refreshTokenFamilyId: family.Id,
                refreshTokenId: existing.Id,
                reasonCode: reason,
                ipAddress: request.IpAddress,
                cancellationToken);

            // Flush the staged security-event outbox row (H-07).
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RefreshFailureResults.InvalidRefreshToken();
        }

        // Reuse detection: if token already consumed, delegate to the
        // session aggregate root. DetectRefreshTokenReuse marks the token
        // as reuse-detected (raising RefreshTokenReuseDetectedDomainEvent
        // with this session id as runtime context) and revokes the
        // session + family. The Domain event is dispatched after SaveChanges
        // by IdentityUnitOfWork -> IDomainEventDispatcher -> the Audit
        // bridge (RefreshTokenReuseAuditHandler) which records to the
        // Audit module.
        if (existing.IsConsumed)
        {
            var reuseNowUtc = _clock.UtcNow;
            session.DetectRefreshTokenReuse(existing, reuseNowUtc, reason: "refresh_token_reuse");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return RefreshFailureResults.ReuseDetected(family.Id);
        }

        // Load the user BEFORE any rotation. Lookup includes soft-deleted rows
        // so a user who was soft-deleted between login and refresh is detected
        // and rejected here rather than silently issued a new access token.
        var nowUtc = _clock.UtcNow;
        var user = await _userRepository.GetByIdAsync(existing.UserId, includeDeleted: true, cancellationToken);

        // Reject deleted / inactive / locked users BEFORE rotation or JWT
        // issuance. No token is rotated and no partial rotated state is added
        // for these failures (Batch R1, H-04 / security-identity.md). The
        // external result is a generic invalid-token error so the caller
        // cannot distinguish user-state from token-state (no enumeration).
        if (user is null || !user.CanRefreshCredentials(nowUtc))
        {
            string reason =
                user is null ? "user_not_found"
                : user.IsDeleted ? "user_deleted"
                : !user.IsActive ? "user_inactive"
                : "user_locked";

            await _securityEventService.TrackRefreshFailedAsync(
                sessionId: session.Id,
                userId: user?.Id ?? existing.UserId,
                refreshTokenFamilyId: family.Id,
                refreshTokenId: existing.Id,
                reasonCode: reason,
                ipAddress: request.IpAddress,
                cancellationToken);

            // Persist the durable security event WITHOUT rotating the token.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RefreshFailureResults.InvalidRefreshToken();
        }

        // Rotate via the UserSession aggregate (atomic: consume old + add new to family).
        var newValue = _refreshTokenGenerator.Generate();
        var newHashHex = _refreshTokenHasher.Hash(newValue);
        var newHashVo = RefreshTokenHash.FromHex(newHashHex);
        var newExpires = nowUtc.Add(_refreshTokenLifetime.RefreshTokenLifetime);

        var newToken = session.RotateRefreshToken(existing, newHashVo, newExpires, request.IpAddress, nowUtc);
        _userSessionRepository.Add(newToken);

        var roleNames = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        var permissionCodes = await _roleRepository.GetPermissionCodesForRolesAsync(user.RoleIds, cancellationToken);
        var jwt = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.DisplayName, roleNames, permissionCodes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshResult(
            AccessToken: jwt.Token,
            RefreshToken: newValue,
            AccessTokenExpiresAt: jwt.ExpiresAt,
            RefreshTokenExpiresAt: newExpires));
    }
}
