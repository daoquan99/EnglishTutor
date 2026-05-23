using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

public sealed class AuditableEntitySaveChangesInterceptor(
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
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

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;
        Guid? userId = currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty
            ? currentUser.UserId
            : null;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                SetCreatedAudit(entry, now, userId);
                SetUpdatedAudit(entry, now, userId, overwrite: false);
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                SetUpdatedAudit(entry, now, userId, overwrite: true);
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            SetProperty(entry, nameof(ISoftDeletable.IsDeleted), true);
            SetProperty(entry, nameof(ISoftDeletable.DeletedAtUtc), now, overwrite: false);
            SetProperty(entry, nameof(ISoftDeletable.DeletedByUserId), userId, overwrite: false);
            SetUpdatedAudit(entry, now, userId, overwrite: true);
        }
    }

    private static void SetCreatedAudit(EntityEntry entry, DateTime now, Guid? userId)
    {
        SetProperty(entry, nameof(IAuditableEntity.CreatedAtUtc), now, overwrite: false);
        SetProperty(entry, nameof(IAuditableEntity.CreatedByUserId), userId, overwrite: false);
    }

    private static void SetUpdatedAudit(EntityEntry entry, DateTime now, Guid? userId, bool overwrite)
    {
        SetProperty(entry, nameof(IAuditableEntity.UpdatedAtUtc), now, overwrite);
        SetProperty(entry, nameof(IAuditableEntity.UpdatedByUserId), userId, overwrite);
    }

    private static void SetProperty<TValue>(EntityEntry entry, string propertyName, TValue value, bool overwrite = true)
    {
        var property = entry.Property(propertyName);
        if (!overwrite && property.CurrentValue is not null && !Equals(property.CurrentValue, default(TValue)))
        {
            return;
        }

        property.CurrentValue = value;
    }
}
