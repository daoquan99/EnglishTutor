namespace EnglishTutor.Modules.Auth.Presentation.Requests;

public sealed record UpdateAuthPermissionRequest(string Description, bool IsEnabled);
