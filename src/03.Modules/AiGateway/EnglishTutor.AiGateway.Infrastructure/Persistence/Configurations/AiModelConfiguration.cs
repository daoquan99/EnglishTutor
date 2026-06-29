using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("ai_models", "aigateway");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasColumnName("display_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ProviderModelId)
            .HasColumnName("provider_model_id")
            .IsRequired()
            .HasMaxLength(200);

        var capabilitiesProperty = builder.Property(e => e.Capabilities)
            .HasColumnName("capabilities")
            .HasConversion(
                values => values.Select(x => x.ToString()).ToArray(),
                values => values.Select(Enum.Parse<AiModelCapability>).ToArray())
            .IsRequired();

        capabilitiesProperty.Metadata.SetValueComparer(
            new ValueComparer<AiModelCapability[]>(
                (left, right) => left != null && right != null && left.SequenceEqual(right),
                values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
                values => values.ToArray()));

        builder.Property(e => e.ThinkingEnabled)
            .HasColumnName("thinking_enabled");

        builder.Property(e => e.Lifecycle)
            .HasColumnName("lifecycle")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.ProviderId);
        builder.HasIndex(e => new { e.ProviderId, e.ProviderModelId }).IsUnique();
        builder.HasIndex(e => new { e.ProviderId, e.IsActive, e.Lifecycle });

        builder.OwnsMany(e => e.ModelVoices, owned =>
        {
            owned.ToTable("ai_model_voices", AiGatewayDbContext.SchemaName);
            owned.WithOwner().HasForeignKey(x => x.ModelId);
            owned.HasKey(x => new { x.ModelId, x.VoiceId });
            owned.Property(x => x.ModelId).HasColumnName("model_id");
            owned.Property(x => x.VoiceId).HasColumnName("voice_id");
            owned.Property(x => x.IsDefault).HasColumnName("is_default").IsRequired();
            owned.HasIndex(x => new { x.ModelId, x.IsDefault });
        });

        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(e => e.DeletedByUserId).HasColumnName("deleted_by_user_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
