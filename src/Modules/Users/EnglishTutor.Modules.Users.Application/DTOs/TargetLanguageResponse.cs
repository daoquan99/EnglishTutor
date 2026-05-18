namespace EnglishTutor.Modules.Users.Application.DTOs;

public sealed record TargetLanguageResponse(
    Guid Id,
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive);
