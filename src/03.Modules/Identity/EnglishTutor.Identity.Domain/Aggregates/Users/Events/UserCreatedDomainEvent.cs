using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Events;

/// <summary>
/// Raised when a new user is created (typically by Identity seed or admin).
/// Consumed by the Audit module.
/// </summary>
public sealed record UserCreatedDomainEvent(
    Guid UserId,
    string Email,
    Guid? CreatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
