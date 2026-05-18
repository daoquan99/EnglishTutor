namespace EnglishTutor.Modules.Users.Application.DTOs;

public sealed record UserProfileResponse(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio);
