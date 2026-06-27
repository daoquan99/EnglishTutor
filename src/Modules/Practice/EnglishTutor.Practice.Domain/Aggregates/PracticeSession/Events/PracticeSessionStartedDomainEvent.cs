using System;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;

public record PracticeSessionStartedDomainEvent(
    Guid SessionId,
    Guid UserId,
    Guid ScenarioId,
    Guid QuotaReservationId,
    Guid RouteLeaseId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
