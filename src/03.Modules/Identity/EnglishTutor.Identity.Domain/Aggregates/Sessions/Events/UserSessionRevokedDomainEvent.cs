using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

/// <summary>
/// Raised when a <c>UserSession</c> is revoked (logout, admin revoke, or
/// security-driven revocation). Carries enough context for downstream
/// modules (Audit, Realtime) to act on the revocation.
/// </summary>
public sealed record UserSessionRevokedDomainEvent(
    Guid UserId,
    Guid SessionId,
    Guid? RevokedByUserId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
