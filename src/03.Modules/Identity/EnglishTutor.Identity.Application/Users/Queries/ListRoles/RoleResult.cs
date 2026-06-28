namespace EnglishTutor.Identity.Application.Users.Queries.ListRoles;

public sealed record RoleResult(
    Guid Id,
    string Name,
    string DisplayName,
    int Priority,
    IReadOnlyList<string> PermissionCodes);
