namespace EnglishTutor.Modules.Auth.Application.DTOs;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName);
