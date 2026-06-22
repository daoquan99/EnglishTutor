using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

/// <summary>
/// Raised when a user explicitly logs out of a single session. Distinct from
/// <c>UserSessionRevokedDomainEvent</c> (which is raised for any revocation
/// cause) so downstream modules can distinguish user-initiated logout from
/// admin or security-driven revocation. Both events are raised on
/// single-session logout; downstream modules decide how to handle each.
/// </summary>
public sealed record UserLoggedOutDomainEvent(
    Guid UserId,
    Guid SessionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
