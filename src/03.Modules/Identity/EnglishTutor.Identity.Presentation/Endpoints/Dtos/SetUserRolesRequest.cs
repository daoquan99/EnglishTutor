namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record SetUserRolesRequest(IReadOnlyList<string> Roles);
