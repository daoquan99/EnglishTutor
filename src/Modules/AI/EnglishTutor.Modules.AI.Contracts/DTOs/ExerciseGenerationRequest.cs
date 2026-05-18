namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record ExerciseGenerationRequest(
    Guid UserId,
    string TargetLanguageCode,
    string UserLevel,
    string Skill,
    int QuestionCount,
    string? Topic);
