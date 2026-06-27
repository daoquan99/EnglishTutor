using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public class AiRoutingRuleConfiguration : IEntityTypeConfiguration<AiRoutingRule>
{
    public void Configure(EntityTypeBuilder<AiRoutingRule> builder)
    {
        // Table name and schema
        builder.ToTable("ai_routing_rules", "aigateway");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        // Properties
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ActivityType)
            .HasColumnName("activity_type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TopicCode)
            .HasColumnName("topic_code")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ScenarioCode)
            .HasColumnName("scenario_code")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PrimaryModelId)
            .HasColumnName("primary_model_id")
            .IsRequired();

        builder.Property(e => e.FallbackModelId)
            .HasColumnName("fallback_model_id")
            .IsRequired(false);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Concurrency token
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes for activity/topic/scenario lookup
        builder.HasIndex(e => new { e.ActivityType, e.TopicCode, e.ScenarioCode, e.IsActive });

        // Audit columns
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

        // Soft delete columns
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(e => e.DeletedByUserId).HasColumnName("deleted_by_user_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
