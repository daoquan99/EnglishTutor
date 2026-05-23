using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;

public sealed record ExerciseStartedDomainEvent(
    Guid UserId,
    Guid AttemptId,
    Guid ExerciseSetId,
    string TargetLanguageCode,
    DateTime StartedAtUtc) : DomainEvent;
