using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Commands.Login;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users.Events;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenLifetime;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;

    public LoginCommandHandler(
        IdentityDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher)
    {
        _db = db;
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
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => EF.Property<string>(u, "Email_Value") == normalizedEmail, cancellationToken);

        if (user is null)
        {
            // Run a dummy verify to keep timing roughly constant — defense against email enumeration.
            _passwordHasher.VerifyPassword(request.Password, "$2a$12$" + new string('x', 53));
            return LoginResults.InvalidCredentials();
        }

        if (user.IsDeleted || !user.IsActive)
        {
            return LoginResults.AccountInactive();
        }

        if (user.IsLockedOut && user.LockoutEndUtc > DateTime.UtcNow)
        {
            return LoginResults.AccountLocked(user.LockoutEndUtc.Value);
        }

        var ok = user.VerifyPassword(request.Password, (plain, hash) =>
            _passwordHasher.VerifyPassword(plain, hash) == PasswordVerificationResult.Success);

        if (!ok)
        {
            await _db.SaveChangesAsync(cancellationToken); // persist updated FailedLoginAttempts / lockout
            return LoginResults.InvalidCredentials();
        }

        var roleIds = user.RoleIds.ToList();
        var roleNames = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        var jwt = _jwtTokenService.IssueAccessToken(user.Id, user.Email.Value, user.DisplayName, roleNames, Array.Empty<string>());

        // Issue refresh token (raw + hash + lifetime via Application abstractions).
        var refreshValue = _refreshTokenGenerator.Generate();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshValue);
        var refreshExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime.RefreshTokenLifetime);
        var familyId = Guid.NewGuid();

        var refreshToken = RefreshToken.Issue(
            user.Id, familyId, refreshTokenHash, refreshExpiresAt, request.IpAddress);

        _db.RefreshTokens.Add(refreshToken);

        // Raise UserLoggedIn event via domain method (handled by EF SaveChangesInterceptor + outbox later)
        user.RaiseDomainEventPublic(new UserLoggedInDomainEvent(user.Id, request.IpAddress));

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResult(jwt.Token, refreshValue, jwt.ExpiresAt));
    }
}
