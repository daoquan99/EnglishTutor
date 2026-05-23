namespace EnglishTutor.Modules.Auth.Application.Shared.DTOs;

public sealed record AuthPermissionResponse(
    Guid Id,
    string Code,
    string Description,
    bool IsEnabled);
