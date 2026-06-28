namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record UpdateRoleRequest(
    string DisplayName,
    int Priority);
