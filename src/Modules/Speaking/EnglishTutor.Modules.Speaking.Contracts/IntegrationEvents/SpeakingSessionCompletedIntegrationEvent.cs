using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

public sealed record SpeakingSessionCompletedIntegrationEvent(
    Guid UserId,
    Guid SessionId,
    string TargetLanguageCode,
    int TotalTurns,
    int OverallScore,
    long DurationSeconds,
    DateTime CompletedAtUtc) : IntegrationEvent;
