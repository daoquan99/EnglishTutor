namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record AiLanguageContext(
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string UserLevel,
    string? Topic);
