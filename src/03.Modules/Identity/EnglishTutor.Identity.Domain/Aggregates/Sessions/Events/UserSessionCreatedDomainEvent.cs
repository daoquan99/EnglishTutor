using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

/// <summary>
/// Raised when a new <c>UserSession</c> is created (typically on successful
/// login). Consumed by the Audit module.
/// </summary>
public sealed record UserSessionCreatedDomainEvent(
    Guid UserId,
    Guid SessionId,
    string DeviceId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
