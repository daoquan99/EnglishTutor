using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Application.Commands.Login;
using EnglishTutor.Modules.Auth.Application.Commands.Register;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.Events;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;
using Xunit;

namespace EnglishTutor.Modules.Auth.UnitTests;

public sealed class AuthDomainTests
{
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
        var user = AuthUser.Register(Email.Create("learner@example.com"), " Learner ");

        Assert.True(user.IsActive);
        Assert.Equal("Learner", user.DisplayName);
        Assert.Contains(user.DomainEvents, domainEvent => domainEvent is UserRegisteredDomainEvent);
    }

    [Fact]
    public void RefreshToken_Create_Binds_User_Session_And_Hash()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, sessionId, "hash", DateTime.UtcNow.AddDays(7));

        Assert.Equal(userId, token.AuthUserId);
        Assert.Equal(sessionId, token.SessionId);
        Assert.Equal("hash", token.TokenHash);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void RefreshToken_Revoke_Stores_Replacement_Id()
    {
        var replacementId = Guid.NewGuid();
        var token = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7));

        token.Revoke(replacementId);

        Assert.True(token.IsRevoked);
        Assert.Equal(replacementId, token.ReplacedByTokenId);
    }

    [Fact]
    public void AuthSession_Create_Normalizes_Device_And_Can_Be_Marked_Used()
    {
        var session = AuthSession.Create(Guid.NewGuid(), " device-1 ", "agent", "127.0.0.1");
        var originalLastUsed = session.LastUsedAtUtc;

        session.MarkUsed();

        Assert.Equal("device-1", session.DeviceId);
        Assert.True(session.LastUsedAtUtc >= originalLastUsed);
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
}
