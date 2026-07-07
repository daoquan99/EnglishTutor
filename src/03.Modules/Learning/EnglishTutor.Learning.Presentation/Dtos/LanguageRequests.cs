namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record AddLanguagePairRequest(
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode);

public sealed record UpdateExplanationLanguageRequest(string ExplanationLanguageCode);
