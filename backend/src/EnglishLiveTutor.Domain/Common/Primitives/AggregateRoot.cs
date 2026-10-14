using EnglishLiveTutor.Domain.Common.Abstractions;

namespace EnglishLiveTutor.Domain.Common.Primitives;

/// <summary>All aggregate roots inherit audit and logical deletion behavior.</summary>
public abstract class AggregateRoot<TId> : Entity<TId>, ISoftDeletable where TId : notnull
{
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public void SoftDelete(DateTimeOffset deletedAtUtc, string? deletedBy)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc.ToUniversalTime();
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedBy = null;
    }
}
