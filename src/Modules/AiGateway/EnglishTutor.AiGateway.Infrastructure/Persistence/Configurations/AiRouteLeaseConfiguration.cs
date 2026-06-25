using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public class AiRouteLeaseConfiguration : IEntityTypeConfiguration<AiRouteLease>
{
    public void Configure(EntityTypeBuilder<AiRouteLease> builder)
    {
        // Table name and schema
        builder.ToTable("ai_route_leases", "aigateway");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        // Properties
        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.RoutingRuleId)
            .HasColumnName("routing_rule_id")
            .IsRequired();

        builder.Property(e => e.ModelId)
            .HasColumnName("model_id")
            .IsRequired();

        builder.Property(e => e.ProviderKeyId)
            .HasColumnName("provider_key_id")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(e => e.ExpiryAtUtc)
            .HasColumnName("expiry_at_utc")
            .IsRequired();

        builder.Property(e => e.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .IsRequired()
            .HasMaxLength(200);

        // Concurrency token
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes
        // Unique user idempotency composite key index
        builder.HasIndex(e => new { e.UserId, e.IdempotencyKey })
            .IsUnique();

        // Route lease cleanup index
        builder.HasIndex(e => new { e.Status, e.ExpiryAtUtc });

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
