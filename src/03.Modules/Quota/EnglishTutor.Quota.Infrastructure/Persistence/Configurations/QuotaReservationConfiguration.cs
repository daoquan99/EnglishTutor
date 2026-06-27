using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Configurations;

public class QuotaReservationConfiguration : IEntityTypeConfiguration<QuotaReservation>
{
    public void Configure(EntityTypeBuilder<QuotaReservation> builder)
    {
        // Table name and schema
        builder.ToTable("quota_reservations", "quota");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(200); // Assuming a reasonable length for idempotency key

        builder.Property(e => e.RequestedMinutes)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ExpiresAtUtc)
            .IsRequired();

        builder.Property(e => e.QuotaDate)
            .IsRequired();

        builder.Property(e => e.ConfirmedAtUtc)
            .IsRequired(false);

        builder.Property(e => e.CancelledAtUtc)
            .IsRequired(false);

        builder.Property(e => e.CorrelationId)
            .IsRequired(false);

        builder.Property(e => e.PracticeSessionId)
            .IsRequired(false);

        // Concurrency token
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes
        // Unique (UserId, IdempotencyKey)
        builder.HasIndex(e => new { e.UserId, e.IdempotencyKey })
            .IsUnique();

        // Index (Status, ExpiresAtUtc) for worker queries
        builder.HasIndex(e => new { e.Status, e.ExpiresAtUtc });

        // Index UserId
        builder.HasIndex(e => e.UserId);

        // Audit columns:
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");
        
        // Soft delete columns:
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(e => e.DeletedByUserId).HasColumnName("deleted_by_user_id");

        builder.Ignore(e => e.DomainEvents);
    }
}