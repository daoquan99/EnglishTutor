using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

public sealed record ConversationPracticeCompletedIntegrationEvent(
    Guid UserId,
    Guid SessionId,
    Guid ConversationScenarioId,
    string TargetLanguageCode,
    int OverallScore,
    int TaskCompletionScore,
    long DurationSeconds,
    DateTime CompletedAtUtc) : IntegrationEvent;
