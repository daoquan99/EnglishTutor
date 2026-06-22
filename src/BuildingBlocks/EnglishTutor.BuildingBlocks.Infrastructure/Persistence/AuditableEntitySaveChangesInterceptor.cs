using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that stamps audit fields
/// (<c>CreatedAtUtc</c>, <c>CreatedByUserId</c>, <c>UpdatedAtUtc</c>,
/// <c>UpdatedByUserId</c>) on every tracked <see cref="Entity"/> right
/// before persistence, and converts a deleted-but-tracked
/// <see cref="AggregateRoot"/> into a soft-delete (<c>IsDeleted = true</c>,
/// <c>DeletedAtUtc</c>, <c>DeletedByUserId</c>) instead of a hard SQL DELETE.
/// </summary>
/// <remarks>
/// <para>Why an interceptor: application code and command handlers MUST NOT
/// set audit fields directly (per AGENTS.md § "Auditable Entities and Soft
/// Delete Rules"). Centralising stamping here keeps the rule enforceable.</para>
/// <para><b>Failure mode:</b> stamping is best-effort and never throws. If
/// the interceptor fails to stamp an entity (e.g., because of a serialization
/// bug), EF Core will still attempt the save — the entity just won't have
/// audit metadata. This trade-off prioritises write availability over
/// audit completeness.</para>
/// </remarks>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUser currentUser,
        IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditFields(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var userId = _currentUser.UserId; // null when system / background job
        var now = _dateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.StampCreated(userId, now);
                    entry.Entity.StampUpdated(userId, now);
                    break;

                case EntityState.Modified:
                    // Only stamp updated-at when a non-soft-delete field
                    // actually changed — otherwise a "delete only" update
                    // would touch the modified-at stamp unnecessarily.
                    if (HasNonSoftDeleteChange(entry))
                    {
                        entry.Entity.StampUpdated(userId, now);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Returns true when any property other than <c>IsDeleted</c>,
    /// <c>DeletedAtUtc</c>, <c>DeletedByUserId</c> was modified.
    /// Soft-delete fields changing alone should NOT bump <c>UpdatedAtUtc</c>.
    /// </summary>
    private static bool HasNonSoftDeleteChange(EntityEntry entry)
    {
        var softDeleteProps = new HashSet<string>
        {
            nameof(AggregateRoot.IsDeleted),
            nameof(AggregateRoot.DeletedAtUtc),
            nameof(AggregateRoot.DeletedByUserId)
        };

        foreach (var prop in entry.Properties)
        {
            if (prop.IsModified && !softDeleteProps.Contains(prop.Metadata.Name))
            {
                return true;
            }
        }

        return false;
    }
}
