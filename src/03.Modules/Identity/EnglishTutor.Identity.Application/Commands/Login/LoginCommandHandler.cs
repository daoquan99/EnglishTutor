using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Events;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Application.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Application.DateTime;

namespace EnglishTutor.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenLifetime;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IIdentitySecurityEventService _securityEventService;
    private readonly IDateTimeProvider _clock;
    private readonly IdentitySecurityOptions _securityOptions;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IIdentitySecurityEventService securityEventService,
        IDateTimeProvider clock,
        IOptions<IdentitySecurityOptions> securityOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenLifetime = refreshTokenLifetime;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _securityEventService = securityEventService;
        _clock = clock;
        _securityOptions = securityOptions.Value;
    }

    public async Task<Result<LoginResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        // Include soft-deleted rows so admin re-activation flows still authenticate.
        var user = await _userRepository.FindByEmailAsync(email, includeDeleted: true, cancellationToken);

        if (user is null)
        {
            // Run a dummy verify to keep timing roughly constant — defense against email enumeration.
            _passwordHasher.VerifyPassword(request.Password, "$2a$12$" + new string('x', 53));

            await _securityEventService.TrackLoginFailedAsync(
                userId: null,
                reasonCode: "user_not_found",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken);

            // Flush the staged security-event outbox row (H-07).
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return LoginFailureResults.InvalidCredentials();
        }

        if (user.IsDeleted || !user.IsActive)
        {
            await _securityEventService.TrackLoginFailedAsync(
                userId: user.Id,
                reasonCode: "account_inactive",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return LoginFailureResults.AccountInactive();
        }

        if (user.IsLockedOut && user.LockoutEndUtc > _clock.UtcNow)
        {
            await _securityEventService.TrackLoginFailedAsync(
                userId: user.Id,
                reasonCode: "account_locked",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return LoginFailureResults.AccountLocked(user.LockoutEndUtc.Value);
        }

        var ok = user.VerifyPassword(
            request.Password,
            (plain, hash) => _passwordHasher.VerifyPassword(plain, hash) == PasswordVerificationResult.Success,
            _securityOptions.MaxFailedLoginAttempts,
            _securityOptions.LockoutDuration);

        if (!ok)
        {
            await _securityEventService.TrackLoginFailedAsync(
                userId: user.Id,
                reasonCode: "invalid_password",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return LoginFailureResults.InvalidCredentials();
        }

        var roleNames = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        var permissionCodes = await _roleRepository.GetPermissionCodesForRolesAsync(user.RoleIds, cancellationToken);

        var jwt = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.DisplayName, roleNames, permissionCodes);

        var userAgent = string.IsNullOrWhiteSpace(request.UserAgent) ? "unknown" : request.UserAgent;
        var deviceId = string.IsNullOrWhiteSpace(request.DeviceId) ? Guid.NewGuid().ToString() : request.DeviceId;
        var deviceName = string.IsNullOrWhiteSpace(request.DeviceName) ? null : request.DeviceName;
        var deviceInfo = DeviceInfo.Create(deviceId, deviceName, userAgent, request.IpAddress);

        var session = UserSession.Create(user.Id, deviceInfo, _clock.UtcNow);

        var refreshValue = _refreshTokenGenerator.Generate();
        var refreshTokenHashHex = _refreshTokenHasher.Hash(refreshValue);
        var refreshExpiresAt = _clock.UtcNow.Add(_refreshTokenLifetime.RefreshTokenLifetime);
        var firstToken = RefreshToken.Issue(
            user.Id, session.Family!.Id, refreshTokenHashHex, refreshExpiresAt, request.IpAddress);
        session.Family.AddToken(firstToken);
        _userSessionRepository.Add(session);
        user.RaiseDomainEventPublic(new UserLoggedInDomainEvent(user.Id, request.IpAddress));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResult(
            AccessToken: jwt.Token,
            RefreshToken: refreshValue,
            AccessTokenExpiresAt: jwt.ExpiresAt,
            RefreshTokenExpiresAt: refreshExpiresAt));
    }
}
