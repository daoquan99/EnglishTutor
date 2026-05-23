namespace EnglishTutor.Modules.Users.Application.Shared.DTOs;

public sealed record LanguageSettingsResponse(
    Guid UserId,
    string NativeLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string ActiveTargetLanguageCode);
