using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Events;

/// <summary>
/// Raised when a user exceeds the failed-login threshold and is locked out.
/// Consumed by the Audit module for security events.
/// </summary>
public sealed record UserLockedOutDomainEvent(
    Guid UserId,
    DateTime LockoutEndUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
