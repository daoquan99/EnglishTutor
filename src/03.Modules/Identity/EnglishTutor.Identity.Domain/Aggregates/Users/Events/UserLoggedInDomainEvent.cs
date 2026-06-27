using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Events;

/// <summary>
/// Raised when a user authenticates successfully. Carries the user's id
/// and the IP from which the request originated (if available) so the
/// Audit module can log auth events.
/// </summary>
public sealed record UserLoggedInDomainEvent(
    Guid UserId,
    string? IpAddress) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
