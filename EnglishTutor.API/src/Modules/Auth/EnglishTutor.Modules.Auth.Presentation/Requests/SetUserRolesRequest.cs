namespace EnglishTutor.Modules.Auth.Presentation.Requests;

public sealed record SetUserRolesRequest(IReadOnlyCollection<Guid> RoleIds);
