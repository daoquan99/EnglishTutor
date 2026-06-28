namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string DisplayName,
    int Priority,
    IReadOnlyList<string> PermissionCodes);
