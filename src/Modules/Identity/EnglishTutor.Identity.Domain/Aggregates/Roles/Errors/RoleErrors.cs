using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;

/// <summary>
/// Domain error factories for role/permission failures.
/// </summary>
public static class RoleErrors
{
    public static Error NotFound(Guid roleId) =>
        new("Identity.RoleNotFound", $"Role '{roleId}' was not found.");

    public static Error NotFoundByName(string name) =>
        new("Identity.RoleNotFound", $"Role '{name}' was not found.");

    public static Error AlreadyExists(string name) =>
        new("Identity.RoleAlreadyExists", $"Role '{name}' already exists.");

    public static Error PermissionDenied(string permissionCode) =>
        new("Identity.PermissionDenied", $"Permission '{permissionCode}' is not granted.");
}
