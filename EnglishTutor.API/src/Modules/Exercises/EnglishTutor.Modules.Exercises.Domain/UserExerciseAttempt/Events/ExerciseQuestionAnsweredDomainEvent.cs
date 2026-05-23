using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;

public sealed record ExerciseQuestionAnsweredDomainEvent(
    Guid UserId,
    Guid AttemptId,
    Guid ExerciseSetId,
    Guid QuestionId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    DateTime AnsweredAtUtc) : DomainEvent;
