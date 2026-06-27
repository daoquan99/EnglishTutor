using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Configurations;

public class UserQuotaRuleConfiguration : IEntityTypeConfiguration<UserQuotaRule>
{
    public void Configure(EntityTypeBuilder<UserQuotaRule> builder)
    {
        // Table name and schema
        builder.ToTable("user_quota_rules", "quota");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired(false); // nullable for global default

        builder.Property(e => e.DailyMaxSessionMinutes)
            .IsRequired();

        builder.Property(e => e.DailyMaxSessions)
            .IsRequired();

        builder.Property(e => e.MaxSingleSessionDuration)
            .IsRequired();

        builder.Property(e => e.EffectiveDate)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        // Concurrency token: Version property
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        // Note: We are not setting ValueGeneratedOnAddOrUpdate because we are managing the version in the domain.

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