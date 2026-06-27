namespace EnglishTutor.BuildingBlocks.Domain.DomainEvents;

/// <summary>
/// Raised after an aggregate root is updated.
/// </summary>
public sealed record AggregateUpdatedDomainEvent(
    Guid AggregateId,
    string AggregateType,
    Guid? UpdatedByUserId,
    System.DateTime UpdatedAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public System.DateTime OccurredAtUtc { get; } = System.DateTime.UtcNow;
}
