using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

public sealed record SpeakingSessionStartedIntegrationEvent(
    Guid UserId,
    Guid SessionId,
    string SessionType,
    string TargetLanguageCode,
    string UserLevel) : IntegrationEvent;
