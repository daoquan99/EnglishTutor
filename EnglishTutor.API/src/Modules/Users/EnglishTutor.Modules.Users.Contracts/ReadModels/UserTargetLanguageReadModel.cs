namespace EnglishTutor.Modules.Users.Contracts.ReadModels;

public sealed record UserTargetLanguageReadModel(
    Guid Id,
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive);
