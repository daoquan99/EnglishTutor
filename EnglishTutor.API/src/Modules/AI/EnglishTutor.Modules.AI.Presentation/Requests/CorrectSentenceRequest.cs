namespace EnglishTutor.Modules.AI.Presentation.Requests;

public sealed record CorrectSentenceRequest(
    string OriginalText,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string UserLevel,
    string? Topic);
