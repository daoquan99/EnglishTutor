namespace EnglishTutor.Modules.Progress.Application.DTOs;

public sealed record ExperienceResponse(
    Guid UserId,
    string TargetLanguageCode,
    int TotalExp,
    string CurrentAppRank);
