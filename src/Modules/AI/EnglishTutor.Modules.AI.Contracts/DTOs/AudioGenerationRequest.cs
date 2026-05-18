namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record AudioGenerationRequest(
    string Text,
    string LanguageCode,
    string? Voice);
