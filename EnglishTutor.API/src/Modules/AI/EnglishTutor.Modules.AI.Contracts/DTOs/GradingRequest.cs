namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record GradingRequest(
    Guid UserId,
    string TargetLanguageCode,
    string UserLevel,
    string Prompt,
    string Answer,
    string Rubric);
