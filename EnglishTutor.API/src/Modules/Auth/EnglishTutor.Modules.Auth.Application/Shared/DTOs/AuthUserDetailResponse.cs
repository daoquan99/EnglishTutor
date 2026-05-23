namespace EnglishTutor.Modules.Auth.Application.Shared.DTOs;

public sealed record AuthUserRoleSummary(Guid RoleId, string Name);

public sealed record AuthUserDetailResponse(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyList<AuthUserRoleSummary> Roles,
    IReadOnlyList<string> EffectivePermissionCodes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
