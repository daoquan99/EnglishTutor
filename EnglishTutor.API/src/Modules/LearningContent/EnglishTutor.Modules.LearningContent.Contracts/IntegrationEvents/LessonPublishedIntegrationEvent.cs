using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;

public sealed record LessonPublishedIntegrationEvent(
    Guid LessonId,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    DateTime PublishedAtUtc) : IntegrationEvent;
