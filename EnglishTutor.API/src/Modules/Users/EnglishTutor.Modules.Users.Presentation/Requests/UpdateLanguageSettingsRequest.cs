namespace EnglishTutor.Modules.Users.Presentation.Requests;

public sealed record UpdateLanguageSettingsRequest(
    string NativeLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string ActiveTargetLanguageCode);
