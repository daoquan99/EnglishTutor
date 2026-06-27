using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        // Table name and schema
        builder.ToTable("ai_models", "aigateway");

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

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Capabilities)
            .HasColumnName("capabilities")
            .IsRequired(); // Natively maps string[] to text[] in PostgreSQL

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Concurrency token
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.ProviderId);

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
