namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string ModuleName,
    string DisplayName);
