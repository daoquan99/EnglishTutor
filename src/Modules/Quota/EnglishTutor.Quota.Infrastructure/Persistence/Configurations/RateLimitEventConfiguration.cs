using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Configurations;

public class RateLimitEventConfiguration : IEntityTypeConfiguration<RateLimitEvent>
{
    public void Configure(EntityTypeBuilder<RateLimitEvent> builder)
    {
        // Table name and schema
        builder.ToTable("rate_limit_events", "quota");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(200); // Assuming a reasonable length for reason

        builder.Property(e => e.LimitType)
            .IsRequired();

        builder.Property(e => e.AttemptedAmount)
            .IsRequired();

        builder.Property(e => e.OccurredAtUtc)
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .IsRequired(false);

        // Indexes
        // Index (UserId, OccurredAtUtc) for querying user's rate limit events over time
        builder.HasIndex(e => new { e.UserId, e.OccurredAtUtc });

        // Audit columns:
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}