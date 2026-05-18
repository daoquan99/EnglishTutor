using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Speaking.Domain.Events;

public sealed record SpeakingSessionCompletedDomainEvent(
    Guid UserId,
    Guid SessionId,
    string TargetLanguageCode,
    int TotalTurns,
    int OverallScore,
    long DurationSeconds,
    DateTime CompletedAtUtc) : DomainEvent;
