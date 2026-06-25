using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Configurations;

public class UserQuotaStateConfiguration : IEntityTypeConfiguration<UserQuotaState>
{
    public void Configure(EntityTypeBuilder<UserQuotaState> builder)
    {
        // Table name and schema
        builder.ToTable("user_quota_states", "quota");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.QuotaDate)
            .IsRequired();

        builder.Property(e => e.ReservedMinutes)
            .IsRequired();

        builder.Property(e => e.UsedMinutes)
            .IsRequired();

        builder.Property(e => e.ReservedSessionCount)
            .IsRequired();

        builder.Property(e => e.UsedSessionCount)
            .IsRequired();

        // Concurrency token
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes: unique (UserId, QuotaDate)
        builder.HasIndex(e => new { e.UserId, e.QuotaDate })
            .IsUnique();

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