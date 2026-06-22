using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Events;

/// <summary>
/// Raised when a user successfully changes their password. Consumed by the
/// Audit module for security events. Does NOT carry the password (or its
/// hash) — only the user id and timestamp.
/// </summary>
public sealed record PasswordChangedDomainEvent(
    Guid UserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
