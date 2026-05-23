namespace EnglishTutor.Modules.Users.Application.Shared.DTOs;

public sealed record TargetLanguageResponse(
    Guid Id,
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive);
