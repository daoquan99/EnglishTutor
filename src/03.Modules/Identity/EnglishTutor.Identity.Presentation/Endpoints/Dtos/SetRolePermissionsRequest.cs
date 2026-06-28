namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record SetRolePermissionsRequest(
    IReadOnlyList<string> PermissionCodes);
