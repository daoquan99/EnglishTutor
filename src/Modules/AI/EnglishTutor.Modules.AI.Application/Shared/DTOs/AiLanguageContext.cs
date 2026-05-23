namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiLanguageContext(
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string UserLevel,
    string? Topic);
