using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;

public sealed record LessonCompletedIntegrationEvent(
    Guid UserId,
    Guid LessonId,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    int DurationSeconds,
    DateTime CompletedAtUtc) : IntegrationEvent;
