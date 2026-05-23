namespace EnglishTutor.Modules.Auth.Application.Shared.DTOs;

public sealed record AuthUserListItem(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyList<Guid> RoleIds,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
