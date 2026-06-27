using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

// Raised when a refresh-token reuse is detected. The entire token family
// is revoked in the same domain operation. Consumed by the Audit module
// (via Identity.Infrastructure.RefreshTokenReuseAuditHandler) as a
// security alert.
//
// Payload updated in Task 22A to carry SessionId and RefreshTokenId so
// the audit table can store the full context (ids only, never the raw
// token).
public sealed record RefreshTokenReuseDetectedDomainEvent(
    Guid UserId,
    Guid SessionId,
    Guid RefreshTokenFamilyId,
    Guid RefreshTokenId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
