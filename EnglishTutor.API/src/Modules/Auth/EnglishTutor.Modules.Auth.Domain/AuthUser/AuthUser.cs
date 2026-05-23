using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Events;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;

namespace EnglishTutor.Modules.Auth.Domain.AuthUser;

public sealed class AuthUser : AggregateRoot<Guid>
{
    private readonly List<AuthUserRole> _roles = [];

    public Email Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<AuthUserRole> Roles => _roles.AsReadOnly();

    private AuthUser() { }

    public static AuthUser Register(Email email, string displayName, DateTime utcNow)
    {
        if (email is null)
        {
            throw new DomainException("Email is required.");
        }

        var user = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = NormalizeDisplayName(displayName),
            IsActive = true
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, email.Value, user.DisplayName));

        return user;
    }

    public void UpdateDisplayName(string displayName, DateTime utcNow)
    {
        DisplayName = NormalizeDisplayName(displayName);
    }

    public void Suspend(DateTime utcNow) => IsActive = false;

    public void Restore(DateTime utcNow) => IsActive = true;

    // Kept for backward compatibility with existing callers; prefer Suspend/Restore.
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void AssignRole(Guid roleId, DateTime utcNow)
    {
        if (roleId == Guid.Empty)
        {
            throw new DomainException("Role id is required.");
        }

        if (_roles.Exists(role => role.RoleId == roleId))
        {
            return;
        }

        _roles.Add(AuthUserRole.Create(Id, roleId, utcNow));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = _roles.Find(role => role.RoleId == roleId);
        if (existing is not null)
        {
            _roles.Remove(existing);
        }
    }

    public void SetRoles(IEnumerable<Guid> roleIds, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(roleIds);

        var desired = roleIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToHashSet();

        var previous = _roles.Select(role => role.RoleId).ToHashSet();

        _roles.RemoveAll(role => !desired.Contains(role.RoleId));

        var existingRoleIds = _roles.Select(role => role.RoleId).ToHashSet();
        var newRoleIds = desired.Where(roleId => !existingRoleIds.Contains(roleId));

        foreach (var roleId in newRoleIds)
        {
            _roles.Add(AuthUserRole.Create(Id, roleId, utcNow));
        }

        if (!previous.SetEquals(desired))
        {
            AddDomainEvent(new UserRolesChangedDomainEvent(Id, desired.ToArray()));
        }
    }

    private static string NormalizeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Display name is required.");
        }

        var normalized = displayName.Trim();
        if (normalized.Length is < 2 or > 100)
        {
            throw new DomainException("Display name must be between 2 and 100 characters.");
        }

        return normalized;
    }
}
