using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Commands.Login;
using EnglishTutor.Modules.Auth.Application.Commands.Register;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Events;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;
using Xunit;

namespace EnglishTutor.Modules.Auth.UnitTests;

public sealed class AuthDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Email_Create_Normalizes_To_Lowercase()
    {
        var email = Email.Create(" Learner@Example.COM ");

        Assert.Equal("learner@example.com", email.Value);
    }

    [Fact]
    public void Email_Create_Rejects_Invalid_Format()
    {
        Assert.Throws<DomainException>(() => Email.Create("not-an-email"));
    }

    [Fact]
    public void AuthUser_Register_Raises_UserRegistered_Event()
    {
        var user = AuthUser.Register(Email.Create("learner@example.com"), " Learner ", UtcNow);

        Assert.True(user.IsActive);
        Assert.Equal("Learner", user.DisplayName);
        Assert.Contains(user.DomainEvents, domainEvent => domainEvent is UserRegisteredDomainEvent);
    }

    [Fact]
    public void RefreshToken_Create_Binds_User_Session_And_Hash()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, sessionId, "hash", UtcNow.AddDays(7), UtcNow);

        Assert.Equal(userId, token.AuthUserId);
        Assert.Equal(sessionId, token.SessionId);
        Assert.Equal("hash", token.TokenHash);
        Assert.True(token.IsActive(UtcNow));
    }

    [Fact]
    public void RefreshToken_Revoke_Stores_Replacement_Id()
    {
        var replacementId = Guid.NewGuid();
        var token = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", UtcNow.AddDays(7), UtcNow);

        token.Revoke(UtcNow, replacementId);

        Assert.True(token.IsRevoked);
        Assert.Equal(replacementId, token.ReplacedByTokenId);
    }

    [Fact]
    public void AuthSession_Create_Normalizes_Device_And_Can_Be_Marked_Used()
    {
        var session = AuthSession.Create(Guid.NewGuid(), " device-1 ", "agent", "127.0.0.1", UtcNow);
        var originalLastUsed = session.LastUsedAtUtc;

        session.MarkUsed(UtcNow.AddMinutes(1));

        Assert.Equal("device-1", session.DeviceId);
        Assert.True(session.LastUsedAtUtc >= originalLastUsed);
    }

    [Fact]
    public void AuthRole_And_Permission_Normalize_Stable_Codes()
    {
        var role = AuthRole.Create(" Admin ", "Full access", isSystem: true, UtcNow);
        var permission = AuthPermission.Create(" AI.Providers.Read ", "Read AI providers", UtcNow);

        Assert.Equal("admin", role.Name);
        Assert.Equal("ai.providers.read", permission.Code);
        Assert.True(role.IsEnabled);
        Assert.True(permission.IsEnabled);
    }

    [Fact]
    public void AuthRolePermission_Binds_Role_And_Permission()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = AuthRolePermission.Create(roleId, permissionId, UtcNow);

        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
    }

    [Fact]
    public void AuthRole_Update_Normalizes_Name_And_Changes_Enabled_State()
    {
        var role = AuthRole.Create(" Content_Admin ", "Manage content", isSystem: false, UtcNow);

        role.Update(" Learning_Admin ", "Manage learning content", isEnabled: false, UtcNow.AddMinutes(1));

        Assert.Equal("learning_admin", role.Name);
        Assert.Equal("Manage learning content", role.Description);
        Assert.False(role.IsEnabled);
    }

    [Fact]
    public void RegisterCommandValidator_Rejects_Weak_Password()
    {
        var validator = new RegisterCommandValidator();

        var result = validator.Validate(new RegisterCommand(
            "learner@example.com",
            "password",
            "password",
            "Learner",
            "device",
            null,
            null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginCommandValidator_Rejects_Invalid_Email()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("bad", "Password123!", "device", null, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RegisterCommandHandler_Creates_User_Session_And_Refresh_Token()
    {
        var authRepository = new FakeAuthRepository();
        var sessionRepository = new FakeAuthSessionRepository();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var handler = CreateRegisterHandler(authRepository, sessionRepository, refreshTokenRepository);

        var result = await handler.Handle(new RegisterCommand(
            "learner@example.com",
            "Password123!",
            "Password123!",
            "Learner",
            "device",
            "agent",
            "127.0.0.1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(authRepository.User);
        Assert.NotNull(sessionRepository.Session);
        Assert.NotNull(refreshTokenRepository.RefreshToken);
        Assert.True(authRepository.SaveRequested);
    }

    [Fact]
    public async Task RegisterCommandHandler_Returns_Error_When_Email_Exists()
    {
        var handler = CreateRegisterHandler(new FakeAuthRepository(emailExists: true), new FakeAuthSessionRepository(), new FakeRefreshTokenRepository());

        var result = await handler.Handle(new RegisterCommand(
            "learner@example.com",
            "Password123!",
            "Password123!",
            "Learner",
            "device",
            null,
            null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.EmailAlreadyExists.Code, result.Error.Code);
    }

    private static RegisterCommandHandler CreateRegisterHandler(
        FakeAuthRepository authRepository,
        FakeAuthSessionRepository sessionRepository,
        FakeRefreshTokenRepository refreshTokenRepository) =>
        new(
            authRepository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeRefreshTokenGenerator(),
            refreshTokenRepository,
            sessionRepository,
            new FakeRefreshTokenCache(),
            authRepository,
            new FakeDateTimeProvider());

    private sealed class FakeAuthRepository(bool emailExists = false) : IAuthRepository, IAuthUnitOfWork
    {
        public AuthUser? User { get; private set; }
        public UserCredential? Credential { get; private set; }
        public bool SaveRequested { get; private set; }

        public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) => Task.FromResult(emailExists);

        public Task<AuthUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(User);

        public Task<AuthUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken) => Task.FromResult(User);

        public Task<UserCredential?> GetCredentialByUserIdAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(Credential);

        public Task AddAsync(AuthUser user, UserCredential credential, CancellationToken cancellationToken)
        {
            User = user;
            Credential = credential;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveRequested = true;
            return Task.FromResult(1);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashedPassword Hash(string password) => HashedPassword.Create("hashed-password");

        public bool Verify(string password, HashedPassword hashedPassword) => true;
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public GeneratedAccessToken Generate(AuthUser user, IReadOnlyCollection<string> permissionCodes) =>
            new("access-token", UtcNow.AddMinutes(15));
    }

    private sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
    {
        public GeneratedRefreshToken Generate() => new("refresh-token", "refresh-token-hash", UtcNow.AddDays(7));
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public RefreshToken? RefreshToken { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) => Task.FromResult(RefreshToken);

        public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            RefreshToken = refreshToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuthSessionRepository : IAuthSessionRepository
    {
        public AuthSession? Session { get; private set; }

        public Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken) => Task.FromResult(Session);

        public Task AddAsync(AuthSession session, CancellationToken cancellationToken)
        {
            Session = session;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenCache : IRefreshTokenCache
    {
        public Task<RefreshTokenCacheReadResult> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken) =>
            Task.FromResult(RefreshTokenCacheReadResult.Available("refresh-token-hash"));

        public Task<bool> StoreTokenHashAsync(Guid sessionId, string deviceId, string tokenHash, DateTime expiresAtUtc, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task RemoveTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => AuthDomainTests.UtcNow;
    }
}
