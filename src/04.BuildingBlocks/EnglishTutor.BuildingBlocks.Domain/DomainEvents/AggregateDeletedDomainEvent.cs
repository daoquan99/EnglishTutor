namespace EnglishTutor.BuildingBlocks.Domain.DomainEvents;

/// <summary>
/// Raised when an aggregate root is soft-deleted via
/// <c>AggregateRoot.MarkDeleted(...)</c>. Carries enough information for the
/// Audit module to log deletion events and for downstream consumers to
/// react to logical deletion.
/// </summary>
public sealed record AggregateDeletedDomainEvent(
    Guid AggregateId,
    string AggregateType,
    Guid? DeletedByUserId,
    System.DateTime DeletedAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public System.DateTime OccurredAtUtc { get; } = System.DateTime.UtcNow;
}
