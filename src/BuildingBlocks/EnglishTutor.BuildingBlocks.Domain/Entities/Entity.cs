namespace EnglishTutor.BuildingBlocks.Domain.Entities;

/// <summary>
/// Base class for all domain entities. Provides identity, equality, and
/// audit metadata inherited by every aggregate root and child entity.
/// </summary>
/// <remarks>
/// <para><b>Audit fields</b> (<see cref="CreatedAtUtc"/>, <see cref="CreatedByUserId"/>,
/// <see cref="UpdatedAtUtc"/>, <see cref="UpdatedByUserId"/>) are populated by the
/// EF Core <c>AuditableEntitySaveChangesInterceptor</c> in Infrastructure —
/// application code and command handlers must not set them directly.</para>
/// <para><b>Stamp methods</b> are <c>protected internal</c>: application code
/// cannot call them, but the Infrastructure assembly can via reflection-free
/// direct invocation (or EF backing fields if the entity is hydrated).</para>
/// </remarks>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    public System.DateTime CreatedAtUtc { get; protected set; }
    public Guid? CreatedByUserId { get; protected set; }
    public System.DateTime UpdatedAtUtc { get; protected set; }
    public Guid? UpdatedByUserId { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Stamps the create-audit fields. Idempotent — only the first call sets
    /// the values. Called by <c>AuditableEntitySaveChangesInterceptor</c>.
    /// </summary>
    internal void StampCreated(Guid? createdByUserId, System.DateTime nowUtc)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }
        CreatedAtUtc = nowUtc;
        CreatedByUserId = createdByUserId;
    }

    /// <summary>
    /// Stamps the update-audit fields. Always overwrites. Called by
    /// <c>AuditableEntitySaveChangesInterceptor</c>.
    /// </summary>
    internal void StampUpdated(Guid? updatedByUserId, System.DateTime nowUtc)
    {
        UpdatedAtUtc = nowUtc;
        UpdatedByUserId = updatedByUserId;
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}
