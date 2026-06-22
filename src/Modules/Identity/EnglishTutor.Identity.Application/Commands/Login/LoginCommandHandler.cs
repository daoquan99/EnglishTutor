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

namespace EnglishTutor.Identity.Application.Commands.Login;

/// <summary>
/// Handles <see cref="LoginCommand"/>: validates credentials, raises a
/// <see cref="UserLoggedInDomainEvent"/>, issues a short-lived access token
/// + a first refresh-token row, and persists via the unit-of-work.
/// <para>
/// Persistence-agnostic: this handler depends only on Application
/// abstractions (<c>IUserRepository</c>, <c>IRoleRepository</c>,
/// <c>IUserSessionRepository</c>, <c>IIdentityUnitOfWork</c>,
/// <c>IPasswordHasher</c>, <c>IJwtTokenService</c>, and the
/// refresh-token Application abstractions). It does NOT reference
/// <c>the DbContext abstraction</c> or any other Infrastructure type.
/// </para>
/// </summary>
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

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher)
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
            return LoginFailureResults.InvalidCredentials();
        }

        if (user.IsDeleted || !user.IsActive)
        {
            return LoginFailureResults.AccountInactive();
        }

        if (user.IsLockedOut && user.LockoutEndUtc > DateTime.UtcNow)
        {
            return LoginFailureResults.AccountLocked(user.LockoutEndUtc.Value);
        }

        var ok = user.VerifyPassword(request.Password, (plain, hash) =>
            _passwordHasher.VerifyPassword(plain, hash) == PasswordVerificationResult.Success);

        if (!ok)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken); // persist updated FailedLoginAttempts / lockout
            return LoginFailureResults.InvalidCredentials();
        }

        var roleNames = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);

        var jwt = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.DisplayName, roleNames, Array.Empty<string>());

        // Build DeviceInfo (no raw PII is stored; user-agent + IP are SHA-256-hashed).
        // userAgent is required by DeviceInfo.Create — default to a sentinel when
        // the HTTP boundary did not supply one.
        var userAgent = string.IsNullOrWhiteSpace(request.UserAgent) ? "unknown" : request.UserAgent;
        var deviceId = string.IsNullOrWhiteSpace(request.DeviceId) ? Guid.NewGuid().ToString() : request.DeviceId;
        var deviceName = string.IsNullOrWhiteSpace(request.DeviceName) ? null : request.DeviceName;
        var deviceInfo = DeviceInfo.Create(deviceId, deviceName, userAgent, request.IpAddress);

        // Create the UserSession (which internally creates a bound RefreshTokenFamily).
        var session = UserSession.Create(user.Id, deviceInfo, DateTime.UtcNow);

        // Issue the first refresh token (raw + hash + lifetime via Application abstractions)
        // and bind it to the newly-created family.
        var refreshValue = _refreshTokenGenerator.Generate();
        var refreshTokenHashHex = _refreshTokenHasher.Hash(refreshValue);
        var refreshExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime.RefreshTokenLifetime);
        var firstToken = RefreshToken.Issue(
            user.Id, session.Family!.Id, session.Id, refreshTokenHashHex, refreshExpiresAt, request.IpAddress);
        session.Family.AddToken(firstToken);

        // Stage the aggregate via the repository. EF cascade tracking handles
        // the family (1:1) and the first token (1:N via the family's
        // Tokens collection) automatically — explicit Add calls for the
        // family and the token would be redundant and risk duplicate
        // inserts.
        _userSessionRepository.Add(session);

        // Raise UserLoggedIn event via the AggregateRoot internal hook
        // (handled by EF SaveChangesInterceptor + outbox later).
        user.RaiseDomainEventPublic(new UserLoggedInDomainEvent(user.Id, request.IpAddress));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResult(jwt.Token, refreshValue, jwt.ExpiresAt));
    }
}