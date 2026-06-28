using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Errors;

/// <summary>
/// Domain error factories for user-related failures. Used by aggregate
/// invariants and (in Slice 2.6) by handlers that translate them into
/// Application-layer <c>Result</c> failures.
/// </summary>
public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("Identity.UserNotFound", $"User '{userId}' was not found.");

    public static Error AlreadyExists(string email) =>
        Error.Conflict("Identity.UserAlreadyExists", $"A user with email '{email}' already exists.");

    public static Error Locked() =>
        Error.Conflict("Identity.UserLocked", "User account is locked.");

    public static Error InvalidCurrentPassword() =>
        Error.Forbidden("Identity.InvalidCurrentPassword", "The current password is invalid.");

    public static Error PrivilegedRoleRequiresOwner() =>
        Error.Forbidden(
            "Identity.PrivilegedRoleRequiresOwner",
            "Only Owner users can manage Owner or Admin role assignments.");

    public static Error PrivilegedUserRequiresOwner() =>
        Error.Forbidden(
            "Identity.PrivilegedUserRequiresOwner",
            "Only Owner users can modify privileged users.");
}
