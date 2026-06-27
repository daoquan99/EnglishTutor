using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class TopicModeConfiguration : IEntityTypeConfiguration<TopicMode>
{
    public void Configure(EntityTypeBuilder<TopicMode> b)
    {
        b.ToTable("topic_modes");
        b.HasKey(tm => tm.Id);

        b.Property(tm => tm.Id).HasColumnName("id");
        b.Property(tm => tm.TopicId).HasColumnName("topic_id").IsRequired();
        b.Property(tm => tm.ModeDefinitionId).HasColumnName("mode_definition_id").IsRequired();
        b.Property(tm => tm.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);
        b.Property(tm => tm.ConfigJson).HasColumnName("config_json").HasColumnType("jsonb");

        // Audit columns:
        b.Property(tm => tm.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(tm => tm.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(tm => tm.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(tm => tm.UpdatedByUserId).HasColumnName("updated_by_user_id");

        // Unique index: topic_id + mode_definition_id
        b.HasIndex(tm => new { tm.TopicId, tm.ModeDefinitionId })
            .HasDatabaseName("ix_topic_modes_topic_id_mode_definition_id")
            .IsUnique();

        // Foreign keys:
        b.HasOne<ModeDefinition>()
            .WithMany()
            .HasForeignKey(tm => tm.ModeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
