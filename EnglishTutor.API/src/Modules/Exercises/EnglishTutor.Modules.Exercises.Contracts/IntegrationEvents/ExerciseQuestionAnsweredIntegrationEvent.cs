using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;

public sealed record ExerciseQuestionAnsweredIntegrationEvent(
    Guid UserId,
    Guid AttemptId,
    Guid ExerciseSetId,
    Guid QuestionId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    DateTime AnsweredAtUtc) : IntegrationEvent;
