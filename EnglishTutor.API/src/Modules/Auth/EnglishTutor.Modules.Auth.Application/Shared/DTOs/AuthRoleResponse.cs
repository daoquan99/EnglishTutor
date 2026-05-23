namespace EnglishTutor.Modules.Auth.Application.Shared.DTOs;

public sealed record AuthRoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsEnabled,
    IReadOnlyList<AuthPermissionResponse> Permissions);
