namespace EnglishTutor.Modules.Users.Presentation.Requests;

public sealed record UpdateProfileRequest(
    string DisplayName,
    string? AvatarUrl,
    string? Bio);
