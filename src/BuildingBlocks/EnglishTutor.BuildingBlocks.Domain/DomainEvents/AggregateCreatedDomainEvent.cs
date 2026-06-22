namespace EnglishTutor.BuildingBlocks.Domain.DomainEvents;

/// <summary>
/// Raised after an aggregate root is first persisted.
/// Carries enough information for the Audit module to log creation events.
/// </summary>
public sealed record AggregateCreatedDomainEvent(
    Guid AggregateId,
    string AggregateType,
    Guid? CreatedByUserId,
    System.DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public System.DateTime OccurredAtUtc { get; } = System.DateTime.UtcNow;
}
