using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public class AiProviderKeyConfiguration : IEntityTypeConfiguration<AiProviderKey>
{
    public void Configure(EntityTypeBuilder<AiProviderKey> builder)
    {
        // Table name and schema
        builder.ToTable("ai_provider_keys", "aigateway");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        // Properties
        builder.Property(e => e.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EncryptedKey)
            .HasColumnName("encrypted_key")
            .IsRequired();

        builder.Property(e => e.KeyMask)
            .HasColumnName("key_mask")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.CooldownUntilUtc)
            .HasColumnName("cooldown_until_utc")
            .IsRequired(false);

        // Concurrency token
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes for provider, status, priority, and cooldown lookup
        builder.HasIndex(e => new { e.ProviderId, e.IsActive, e.Priority, e.CooldownUntilUtc });

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
