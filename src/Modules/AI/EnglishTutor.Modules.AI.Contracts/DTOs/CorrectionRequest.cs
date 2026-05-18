namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record CorrectionRequest(
    Guid UserId,
    string OriginalText,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string UserLevel,
    string? Topic);
