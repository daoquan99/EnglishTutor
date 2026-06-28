namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record CreateRoleRequest(
    string Name,
    string DisplayName,
    int Priority,
    IReadOnlyList<string>? PermissionCodes);
