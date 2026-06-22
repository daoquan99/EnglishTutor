using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

/// <summary>
/// Raised when a refresh-token reuse is detected. The entire token family
/// is revoked in the same domain operation. Consumed by the Audit module
/// as a security alert.
/// </summary>
public sealed record RefreshTokenReuseDetectedDomainEvent(
    Guid UserId,
    Guid FamilyId,
    string? IpAddress) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
