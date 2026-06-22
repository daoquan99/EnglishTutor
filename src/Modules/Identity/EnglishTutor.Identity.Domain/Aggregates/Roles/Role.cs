namespace EnglishTutor.Identity.Domain.Aggregates.Roles;

/// <summary>
/// Simple lookup entity representing a role. NOT an aggregate root — roles
/// are global configuration data, not independently-lifecycle aggregates.
/// </summary>
public sealed class Role : EnglishTutor.BuildingBlocks.Domain.Entities.Entity
{
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public int Priority { get; private set; }

    // EF Core parameterless constructor.
    private Role() { }

    public Role(string name, string displayName, int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name is required.", nameof(name));
        }
        Name = name.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Name : displayName.Trim();
        Priority = priority;
    }

    public static class WellKnownNames
    {
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
