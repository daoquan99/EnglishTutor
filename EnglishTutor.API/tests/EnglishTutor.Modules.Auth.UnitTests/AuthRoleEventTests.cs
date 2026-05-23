using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Events;
using Xunit;

namespace EnglishTutor.Modules.Auth.UnitTests;

public sealed class AuthRoleEventTests
{
    private static readonly DateTime BaseUtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RaisePermissionsChanged_Adds_Domain_Event_With_Affected_Users()
    {
        var role = AuthRole.Create("editor", "Content editor", isSystem: false, BaseUtcNow);
        role.ClearDomainEvents();
        var affected = new[] { Guid.NewGuid(), Guid.NewGuid() };

        role.RaisePermissionsChanged(affected);

        var domainEvent = Assert.Single(role.DomainEvents);
        var changed = Assert.IsType<RolePermissionsChangedDomainEvent>(domainEvent);
        Assert.Equal(role.Id, changed.RoleId);
        Assert.Equal(role.Name, changed.RoleName);
        Assert.Equal(affected, changed.AffectedUserIds);
    }

    [Fact]
    public void RaisePermissionsChanged_Allows_Empty_Affected_Users()
    {
        var role = AuthRole.Create("editor", "Content editor", isSystem: false, BaseUtcNow);
        role.ClearDomainEvents();

        role.RaisePermissionsChanged(Array.Empty<Guid>());

        var domainEvent = Assert.Single(role.DomainEvents);
        var changed = Assert.IsType<RolePermissionsChangedDomainEvent>(domainEvent);
        Assert.Empty(changed.AffectedUserIds);
    }
}
