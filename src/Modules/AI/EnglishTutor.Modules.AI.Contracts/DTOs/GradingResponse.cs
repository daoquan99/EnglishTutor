namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record GradingResponse(
    int Score,
    string Feedback,
    IReadOnlyDictionary<string, int> RubricScores);
