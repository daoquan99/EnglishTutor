using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;

/// <summary>
/// Domain error factories for role/permission failures.
/// </summary>
public static class RoleErrors
{
    public static Error NotFound(Guid roleId) =>
        Error.NotFound("Identity.RoleNotFound", $"Role '{roleId}' was not found.");

    public static Error NotFoundByName(string name) =>
        Error.NotFound("Identity.RoleNotFound", $"Role '{name}' was not found.");

    public static Error AlreadyExists(string name) =>
        Error.Conflict("Identity.RoleAlreadyExists", $"Role '{name}' already exists.");

    public static Error PermissionDenied(string permissionCode) =>
        Error.Forbidden("Identity.PermissionDenied", $"Permission '{permissionCode}' is not granted.");

    public static Error PermissionNotFound(string code) =>
        Error.NotFound("Identity.PermissionNotFound", $"Permission '{code}' was not found.");

    public static Error WellKnownRoleNameCannotChange(string name) =>
        Error.Conflict("Identity.WellKnownRoleNameCannotChange", $"Well-known role '{name}' cannot be renamed.");
}
