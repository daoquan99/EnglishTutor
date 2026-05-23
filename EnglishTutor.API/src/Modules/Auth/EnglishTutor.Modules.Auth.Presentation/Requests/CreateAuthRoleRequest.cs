namespace EnglishTutor.Modules.Auth.Presentation.Requests;

public sealed record CreateAuthRoleRequest(
    string Name,
    string Description,
    bool IsEnabled,
    IReadOnlyCollection<Guid> PermissionIds);
