namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record UserManagementResponse(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsLockedOut,
    DateTime? LockoutEndUtc,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
