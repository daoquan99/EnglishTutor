namespace EnglishTutor.Modules.Progress.Application.Queries.GetExperience;

public sealed record ExperienceResponse(
    Guid UserId,
    string TargetLanguageCode,
    int TotalExp,
    string CurrentAppRank);
