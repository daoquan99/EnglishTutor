namespace EnglishTutor.Modules.Users.Application.Shared.DTOs;

public sealed record UserProfileResponse(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio);
