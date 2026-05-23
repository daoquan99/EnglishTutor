namespace EnglishTutor.Modules.Auth.Presentation.Requests;

public sealed record UpdateAuthRoleRequest(
    string Name,
    string Description,
    bool IsEnabled,
    IReadOnlyCollection<Guid> PermissionIds);
