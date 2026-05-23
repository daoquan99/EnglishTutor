namespace EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
