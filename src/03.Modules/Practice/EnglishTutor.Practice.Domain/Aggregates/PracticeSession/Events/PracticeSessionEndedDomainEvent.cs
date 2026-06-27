using System;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;

public record PracticeSessionEndedDomainEvent(
    Guid SessionId,
    Guid UserId,
    Guid QuotaReservationId,
    Guid RouteLeaseId,
    PracticeSessionStatus FinalStatus,
    int DurationSeconds,
    string EndReason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
