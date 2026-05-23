using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;

public sealed record ConversationScenarioCompletedIntegrationEvent(
    Guid UserId,
    Guid ConversationScenarioId,
    string TargetLanguageCode,
    string Level,
    int DurationSeconds,
    DateTime CompletedAtUtc) : IntegrationEvent;
