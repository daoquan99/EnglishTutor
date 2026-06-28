namespace EnglishTutor.Identity.Application.Users.Queries.GetUser;

public sealed record UserDetailResult(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsLockedOut,
    DateTime? LockoutEndUtc,
    int FailedLoginAttempts,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
