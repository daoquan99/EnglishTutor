namespace EnglishTutor.BuildingBlocks.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IDomainEventHolder, ISoftDeletable where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAtUtc { get; protected set; }
    public Guid? DeletedByUserId { get; protected set; }

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    protected void AddDomainEvent(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void MarkDeleted(DateTime deletedAtUtc, Guid? deletedByUserId)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        DeletedByUserId = deletedByUserId;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedByUserId = null;
    }
}
