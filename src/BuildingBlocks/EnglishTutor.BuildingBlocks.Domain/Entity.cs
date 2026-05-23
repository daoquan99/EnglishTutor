namespace EnglishTutor.BuildingBlocks.Domain;

public abstract class Entity<TId> : IAuditableEntity, IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAtUtc { get; protected set; }
    public Guid? CreatedByUserId { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }
    public Guid? UpdatedByUserId { get; protected set; }

    protected Entity() { }

    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Equals(entity);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null || GetType() != other.GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (HasDefaultId() || other.HasDefaultId())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);

    private bool HasDefaultId() =>
        EqualityComparer<TId>.Default.Equals(Id, default!);

    protected static void CheckRule(Rules.IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new Exceptions.BusinessRuleValidationException(rule);
    }
}
