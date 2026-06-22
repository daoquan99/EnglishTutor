using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

public class UserTests
{
    private static User CreateValidUser(string email = "user@test.com")
    {
        var hash = HashedPassword.FromNewHash("$2a$12$abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQR");
        return User.Create(Email.Create(email), hash, "Test User", Array.Empty<Guid>(), createdByUserId: null);
    }

    [Fact]
    public void Create_Should_Produce_User_With_Email_DisplayName_And_Empty_Role_List()
    {
        var user = CreateValidUser();
        user.Email.Value.Should().Be("user@test.com");
        user.DisplayName.Should().Be("Test User");
        user.RoleIds.Should().BeEmpty();
        user.IsActive.Should().BeTrue();
        user.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_With_Correct_Password_Should_Return_True()
    {
        var user = CreateValidUser();
        var ok = user.VerifyPassword("plain", (plain, hash) => plain == "plain");
        ok.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void VerifyPassword_With_Wrong_Password_Should_Return_False_And_Increment()
    {
        var user = CreateValidUser();
        var ok = user.VerifyPassword("plain", (_, _) => false);
        ok.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void Five_Failed_Attempts_Should_Lockout_For_15_Minutes()
    {
        var user = CreateValidUser();
        for (var i = 0; i < 5; i++)
        {
            user.VerifyPassword("plain", (_, _) => false);
        }
        user.IsLockedOut.Should().BeTrue();
        user.LockoutEndUtc.Should().NotBeNull();
        user.LockoutEndUtc!.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Locked_User_Fails_Verification_Even_With_Correct_Password()
    {
        var user = CreateValidUser();
        user.Lockout(DateTime.UtcNow.AddMinutes(5));
        var ok = user.VerifyPassword("plain", (_, _) => true);
        ok.Should().BeFalse();
    }

    [Fact]
    public void Unlock_Should_Clear_Lockout_State_And_Reset_Counter()
    {
        var user = CreateValidUser();
        user.Lockout(DateTime.UtcNow.AddMinutes(5));
        user.Unlock();
        user.IsLockedOut.Should().BeFalse();
        user.LockoutEndUtc.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void AssignRole_Should_Be_Idempotent()
    {
        var user = CreateValidUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);
        user.AssignRole(roleId);
        user.RoleIds.Should().ContainSingle().Which.Should().Be(roleId);
    }

    [Fact]
    public void ChangePassword_Should_Reset_Lockout()
    {
        var user = CreateValidUser();
        user.Lockout(DateTime.UtcNow.AddMinutes(5));
        var newHash = HashedPassword.FromNewHash("$2a$12$newsaltandhash1234567890ABCDEFGHIJKLMNOPQRSTUV");
        user.ChangePassword(newHash);
        user.IsLockedOut.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
    }
}
