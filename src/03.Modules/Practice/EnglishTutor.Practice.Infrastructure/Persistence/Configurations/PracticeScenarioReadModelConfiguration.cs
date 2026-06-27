using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Configurations;

internal sealed class PracticeScenarioReadModelConfiguration : IEntityTypeConfiguration<PracticeScenarioReadModel>
{
    public void Configure(EntityTypeBuilder<PracticeScenarioReadModel> b)
    {
        b.ToTable("scenario_read_models");
        b.HasKey(s => s.Id);

        b.Property(s => s.Id).HasColumnName("id");
        b.Property(s => s.TopicId).HasColumnName("topic_id").IsRequired();
        b.Property(s => s.TopicCode).HasColumnName("topic_code").HasMaxLength(100).IsRequired();
        b.Property(s => s.TopicTitle).HasColumnName("topic_title").HasMaxLength(255).IsRequired();
        b.Property(s => s.ModeDefinitionId).HasColumnName("mode_definition_id").IsRequired();
        b.Property(s => s.ModeCode).HasColumnName("mode_code").HasMaxLength(50).IsRequired();
        b.Property(s => s.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        b.Property(s => s.LearnerFacingInstructions).HasColumnName("learner_facing_instructions").IsRequired();

        // Audit columns:
        b.Property(s => s.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(s => s.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(s => s.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(s => s.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
