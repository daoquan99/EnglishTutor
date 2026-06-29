using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Configurations;

public sealed class AiVoiceConfiguration : IEntityTypeConfiguration<AiVoice>
{
    public void Configure(EntityTypeBuilder<AiVoice> builder)
    {
        builder.ToTable("ai_voices", AiGatewayDbContext.SchemaName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(x => x.VoiceId).HasColumnName("voice_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Style).HasColumnName("style").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken().IsRequired();

        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.ProviderId, x.VoiceId }).IsUnique();
        builder.HasIndex(x => new { x.ProviderId, x.IsActive, x.DisplayName, x.Id });
    }
}
