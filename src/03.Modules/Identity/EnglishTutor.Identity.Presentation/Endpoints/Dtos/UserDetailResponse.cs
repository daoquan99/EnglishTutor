namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record UserDetailResponse(
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
