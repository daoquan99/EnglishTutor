namespace EnglishTutor.Identity.Application.Users.Queries.ListUsers;

public sealed record UserListItemResult(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsLockedOut,
    DateTime? LockoutEndUtc,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
