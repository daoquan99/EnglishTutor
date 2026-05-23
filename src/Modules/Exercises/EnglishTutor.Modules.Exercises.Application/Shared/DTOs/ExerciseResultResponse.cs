namespace EnglishTutor.Modules.Exercises.Application.Shared.DTOs;

public sealed record ExerciseResultResponse(
    Guid AttemptId,
    Guid ExerciseSetId,
    string ExerciseType,
    string TargetLanguageCode,
    int TotalScore,
    int CorrectCount,
    int TotalQuestions,
    int TimeTakenSeconds,
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    IReadOnlyList<ExerciseAnswerResultResponse> Answers);

public sealed record ExerciseAnswerResultResponse(
    Guid QuestionId,
    string QuestionType,
    string Prompt,
    string UserAnswer,
    string? CorrectAnswer,
    bool IsCorrect,
    int Score,
    string? Feedback,
    string? Explanation,
    DateTime AnsweredAtUtc);
