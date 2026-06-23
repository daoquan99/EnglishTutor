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

    public RefreshCommandHandler(
        IUserSessionRepository userSessionRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IIdentitySecurityEventService securityEventService)
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

            return RefreshFailureResults.InvalidRefreshToken();
        }

        var session = snapshot.Session;
        var family = snapshot.Family;
        var existing = snapshot.ActiveToken;

        if (session.RevokedAtUtc is not null || family.IsRevoked || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            string reason = "invalid_token";
            if (existing.ExpiresAtUtc <= DateTime.UtcNow) reason = "token_expired";
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
            var nowUtc = DateTime.UtcNow;
            session.DetectRefreshTokenReuse(existing, nowUtc, reason: "refresh_token_reuse");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return RefreshFailureResults.ReuseDetected(family.Id);
        }

        // Rotate via the UserSession aggregate (atomic: consume old + add new to family).
        var newValue = _refreshTokenGenerator.Generate();
        var newHashHex = _refreshTokenHasher.Hash(newValue);
        var newHashVo = RefreshTokenHash.FromHex(newHashHex);
        var newExpires = DateTime.UtcNow.Add(_refreshTokenLifetime.RefreshTokenLifetime);

        var newToken = session.RotateRefreshToken(existing, newHashVo, newExpires, request.IpAddress, DateTime.UtcNow);
        _userSessionRepository.Add(newToken);

        // Issue new access token. User lookup includes soft-deleted rows so a
        // refresh request still resolves a user who has been soft-deleted
        // between login and refresh.
        var user = await _userRepository.GetByIdAsync(existing.UserId, includeDeleted: true, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RefreshResult>(new Error(
                "Identity.UserNotFound",
                "User associated with refresh token no longer exists."));
        }

        var roleNames = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        var permissionCodes = await _roleRepository.GetPermissionCodesForRolesAsync(user.RoleIds, cancellationToken);
        var jwt = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.DisplayName, roleNames, permissionCodes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshResult(jwt.Token, newValue, jwt.ExpiresAt));
    }
}