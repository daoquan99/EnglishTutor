using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;

public sealed record ExerciseStartedIntegrationEvent(
    Guid UserId,
    Guid AttemptId,
    Guid ExerciseSetId,
    string TargetLanguageCode,
    DateTime StartedAtUtc) : IntegrationEvent;
