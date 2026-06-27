using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Configurations;

public class UsageLogConfiguration : IEntityTypeConfiguration<UsageLog>
{
    public void Configure(EntityTypeBuilder<UsageLog> builder)
    {
        // Table name and schema
        builder.ToTable("usage_logs", "quota");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.QuotaReservationId)
            .IsRequired();

        builder.Property(e => e.PracticeSessionId)
            .IsRequired(false);

        builder.Property(e => e.DurationMinutes)
            .IsRequired();

        builder.Property(e => e.OccurredAtUtc)
            .IsRequired();

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(200); // Assuming a reasonable length for source

        // Indexes: (UserId, OccurredAtUtc) as per requirements
        builder.HasIndex(e => new { e.UserId, e.OccurredAtUtc });

        // Optional index on PracticeSessionId
        builder.HasIndex(e => e.PracticeSessionId);

        // Audit columns:
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}