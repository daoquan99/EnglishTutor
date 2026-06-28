namespace EnglishTutor.Identity.Application.Users.Queries.ListPermissions;

public sealed record PermissionResult(
    Guid Id,
    string Code,
    string ModuleName,
    string DisplayName);
