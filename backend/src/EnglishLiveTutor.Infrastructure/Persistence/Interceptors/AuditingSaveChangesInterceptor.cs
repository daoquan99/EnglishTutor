using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EnglishLiveTutor.Infrastructure.Persistence.Interceptors;

/// <summary>Tracks audit fields on all entities and prevents accidental hard deletion of roots.</summary>
public sealed class AuditingSaveChangesInterceptor(TimeProvider clock, ICurrentUser actor)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? db)
    {
        if (db is null) return;
        db.ChangeTracker.DetectChanges();
        var now = clock.GetUtcNow();
        var userId = actor.UserId ?? "system";
        foreach (var entry in db.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
                throw new InvalidOperationException(
                    "Hard deleting Aggregate Roots is forbidden. Call SoftDelete() instead.");
        }

        foreach (var entry in db.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                // Preserve intentionally specified historical timestamps during imports/seeding.
                if (entry.Entity.CreatedAtUtc == default)
                    entry.Property("CreatedAt").CurrentValue = now;
                entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("CreatedAt").IsModified = false;
                entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                entry.Property("UpdatedAt").CurrentValue = now;
                entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = userId;
            }
        }
    }
}
