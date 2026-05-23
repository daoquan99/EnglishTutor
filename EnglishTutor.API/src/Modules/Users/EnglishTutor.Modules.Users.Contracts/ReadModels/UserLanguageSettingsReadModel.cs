namespace EnglishTutor.Modules.Users.Contracts.ReadModels;

public sealed record UserLanguageSettingsReadModel(
    Guid UserId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string CurrentLevel,
    string TargetLevel);
