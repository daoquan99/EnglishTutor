namespace EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;

public sealed record SubmitAnswerResponse(
    Guid AttemptId,
    Guid QuestionId,
    bool IsCorrect,
    int Score,
    string? Feedback,
    string? Explanation,
    DateTime AnsweredAtUtc);
