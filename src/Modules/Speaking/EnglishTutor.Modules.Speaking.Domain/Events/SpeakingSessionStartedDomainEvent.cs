using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Speaking.Domain.Events;

public sealed record SpeakingSessionStartedDomainEvent(
    Guid UserId,
    Guid SessionId,
    string SessionType,
    string TargetLanguageCode,
    string UserLevel) : DomainEvent;
