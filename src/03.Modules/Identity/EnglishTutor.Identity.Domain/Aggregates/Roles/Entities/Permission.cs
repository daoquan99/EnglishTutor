namespace EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;

/// <summary>
/// Simple lookup entity representing a permission code. NOT an aggregate root.
/// </summary>
public sealed class Permission : EnglishTutor.BuildingBlocks.Domain.Entities.Entity
{
    public string Code { get; private set; } = string.Empty;
    public string ModuleName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    private Permission() { }

    public Permission(string code, string moduleName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Permission code is required.", nameof(code));
        }
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name is required.", nameof(moduleName));
        }

        Code = code.Trim();
        ModuleName = moduleName.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Code : displayName.Trim();
    }
}
