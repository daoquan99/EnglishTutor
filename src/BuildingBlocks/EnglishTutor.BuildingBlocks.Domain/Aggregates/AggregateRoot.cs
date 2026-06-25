using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.BuildingBlocks.Domain.Aggregates;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Soft-delete flag. Set by <see cref="MarkDeleted"/>.</summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>UTC timestamp when soft-deleted. Null if not deleted.</summary>
    public System.DateTime? DeletedAtUtc { get; protected set; }

    /// <summary>User who soft-deleted this aggregate. Null if not deleted.</summary>
    public Guid? DeletedByUserId { get; protected set; }

    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Internal entry-point for Infrastructure (handlers, interceptors) to raise
    /// domain events without making <see cref="RaiseDomainEvent"/> public.
    /// Used sparingly — prefer adding domain methods that raise events from
    /// inside the aggregate (rich domain behavior).
    /// </summary>
    internal void RaiseDomainEventPublic(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks this aggregate as soft-deleted. Idempotent — calling twice
    /// does not raise duplicate events.
    /// </summary>
    public void MarkDeleted(Guid? deletedByUserId, System.DateTime nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        DeletedByUserId = deletedByUserId;

        RaiseDomainEvent(new AggregateDeletedDomainEvent(
            AggregateId: Id,
            AggregateType: GetType().Name,
            DeletedByUserId: deletedByUserId,
            DeletedAtUtc: nowUtc));
    }

    /// <summary>
    /// Reverses a soft-delete. Intended for admin restore flows.
    /// Does NOT raise a domain event — restoration is a manual admin action
    /// that should be audited via the Audit module's API, not via events.
    /// </summary>
    public void Undelete()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedByUserId = null;
    }
}
