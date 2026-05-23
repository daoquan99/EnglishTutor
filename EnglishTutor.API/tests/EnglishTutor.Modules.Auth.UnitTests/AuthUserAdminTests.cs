using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Events;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;
using Xunit;

namespace EnglishTutor.Modules.Auth.UnitTests;

public sealed class AuthUserAdminTests
{
    private static readonly DateTime BaseUtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UpdateDisplayName_Trims_And_Sets()
    {
        var user = CreateUser();

        user.UpdateDisplayName("  New Name  ", BaseUtcNow);

        Assert.Equal("New Name", user.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_Throws_When_Too_Short()
    {
        var user = CreateUser();

        Assert.Throws<DomainException>(() => user.UpdateDisplayName("a", BaseUtcNow));
    }

    [Fact]
    public void Suspend_Sets_IsActive_False()
    {
        var user = CreateUser();
        Assert.True(user.IsActive);

        user.Suspend(BaseUtcNow);

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Restore_Sets_IsActive_True()
    {
        var user = CreateUser();
        user.Suspend(BaseUtcNow);

        user.Restore(BaseUtcNow);

        Assert.True(user.IsActive);
    }

    [Fact]
    public void AssignRole_Adds_Role_When_Not_Present()
    {
        var user = CreateUser();
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId, BaseUtcNow);

        Assert.Single(user.Roles);
        Assert.Equal(roleId, user.Roles.First().RoleId);
    }

    [Fact]
    public void AssignRole_Is_Idempotent()
    {
        var user = CreateUser();
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId, BaseUtcNow);
        user.AssignRole(roleId, BaseUtcNow);

        Assert.Single(user.Roles);
    }

    [Fact]
    public void AssignRole_Throws_When_RoleId_Empty()
    {
        var user = CreateUser();

        Assert.Throws<DomainException>(() => user.AssignRole(Guid.Empty, BaseUtcNow));
    }

    [Fact]
    public void RemoveRole_Removes_When_Present()
    {
        var user = CreateUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, BaseUtcNow);

        user.RemoveRole(roleId);

        Assert.Empty(user.Roles);
    }

    [Fact]
    public void RemoveRole_Is_Idempotent_When_Absent()
    {
        var user = CreateUser();

        user.RemoveRole(Guid.NewGuid());

        Assert.Empty(user.Roles);
    }

    [Fact]
    public void SetRoles_Replaces_And_Dedupes()
    {
        var user = CreateUser();
        var existing = Guid.NewGuid();
        user.AssignRole(existing, BaseUtcNow);
        var keep = Guid.NewGuid();
        var newRole = Guid.NewGuid();

        user.SetRoles([keep, newRole, keep], BaseUtcNow);

        Assert.Equal(2, user.Roles.Count);
        Assert.Contains(user.Roles, role => role.RoleId == keep);
        Assert.Contains(user.Roles, role => role.RoleId == newRole);
        Assert.DoesNotContain(user.Roles, role => role.RoleId == existing);
    }

    [Fact]
    public void SetRoles_Empty_Clears_All()
    {
        var user = CreateUser();
        user.AssignRole(Guid.NewGuid(), BaseUtcNow);
        user.AssignRole(Guid.NewGuid(), BaseUtcNow);

        user.SetRoles([], BaseUtcNow);

        Assert.Empty(user.Roles);
    }

    [Fact]
    public void SetRoles_Ignores_Empty_Guids()
    {
        var user = CreateUser();
        var validRoleId = Guid.NewGuid();

        user.SetRoles([Guid.Empty, validRoleId, Guid.Empty], BaseUtcNow);

        Assert.Single(user.Roles);
        Assert.Equal(validRoleId, user.Roles.First().RoleId);
    }

    [Fact]
    public void SetRoles_Raises_UserRolesChangedDomainEvent_When_Set_Differs()
    {
        var user = CreateUser();
        user.ClearDomainEvents();
        var newRoleId = Guid.NewGuid();

        user.SetRoles([newRoleId], BaseUtcNow);

        var domainEvent = Assert.Single(user.DomainEvents);
        var rolesChanged = Assert.IsType<UserRolesChangedDomainEvent>(domainEvent);
        Assert.Equal(user.Id, rolesChanged.UserId);
        Assert.Equal(new[] { newRoleId }, rolesChanged.NewRoleIds);
    }

    [Fact]
    public void SetRoles_Does_Not_Raise_Event_When_Set_Unchanged()
    {
        var user = CreateUser();
        var roleId = Guid.NewGuid();
        user.SetRoles([roleId], BaseUtcNow);
        user.ClearDomainEvents();

        user.SetRoles([roleId], BaseUtcNow);

        Assert.Empty(user.DomainEvents);
    }

    private static AuthUser CreateUser() =>
        AuthUser.Register(Email.Create("learner@example.com"), "Learner", BaseUtcNow);
}
