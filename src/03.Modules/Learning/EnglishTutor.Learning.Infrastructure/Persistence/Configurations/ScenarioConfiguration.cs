using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
{
    public void Configure(EntityTypeBuilder<Scenario> b)
    {
        b.ToTable("scenarios");
        b.HasKey(s => s.Id);

        b.Property(s => s.Id).HasColumnName("id");
        b.Property(s => s.TopicId).HasColumnName("topic_id").IsRequired();
        b.Property(s => s.ModeDefinitionId).HasColumnName("mode_definition_id").IsRequired();
        b.Property(s => s.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        b.Property(s => s.Description).HasColumnName("description");
        b.Property(s => s.DifficultyLevel).HasColumnName("difficulty_level").HasMaxLength(50).IsRequired();
        b.Property(s => s.PromptTemplate).HasColumnName("prompt_template").IsRequired();
        b.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        // Audit columns:
        b.Property(s => s.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(s => s.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(s => s.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(s => s.UpdatedByUserId).HasColumnName("updated_by_user_id");
        
        // Soft delete columns:
        b.Property(s => s.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        b.Property(s => s.DeletedAtUtc).HasColumnName("deleted_at_utc");
        b.Property(s => s.DeletedByUserId).HasColumnName("deleted_by_user_id");

        b.Ignore(s => s.DomainEvents);

        // Unique index: topic_id + mode_definition_id + name
        b.HasIndex(s => new { s.TopicId, s.ModeDefinitionId, s.Name })
            .HasDatabaseName("ix_scenarios_topic_id_mode_definition_id_name")
            .IsUnique()
            .HasFilter("is_deleted = false");

        // Decoupled foreign keys:
        b.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(s => s.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<ModeDefinition>()
            .WithMany()
            .HasForeignKey(s => s.ModeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
