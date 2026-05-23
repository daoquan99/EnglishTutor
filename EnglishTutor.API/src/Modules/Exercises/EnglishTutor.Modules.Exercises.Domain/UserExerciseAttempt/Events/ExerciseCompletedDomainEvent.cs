using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;

public sealed record ExerciseCompletedWrongAnswer(
    Guid QuestionId,
    string Prompt,
    string UserAnswer,
    string CorrectAnswer,
    string? Explanation,
    string QuestionType);

public sealed record ExerciseCompletedDomainEvent(
    Guid UserId,
    Guid ExerciseSetId,
    Guid AttemptId,
    string TargetLanguageCode,
    string ExerciseType,
    int Score,
    int CorrectCount,
    int TotalQuestions,
    int TimeTakenSeconds,
    IReadOnlyCollection<ExerciseCompletedWrongAnswer> WrongAnswers,
    DateTime CompletedAtUtc) : DomainEvent;
