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
        new("Identity.UserNotFound", $"User '{userId}' was not found.");

    public static Error AlreadyExists(string email) =>
        new("Identity.UserAlreadyExists", $"A user with email '{email}' already exists.");

    public static Error Locked() =>
        new("Identity.UserLocked", "User account is locked.");
}
