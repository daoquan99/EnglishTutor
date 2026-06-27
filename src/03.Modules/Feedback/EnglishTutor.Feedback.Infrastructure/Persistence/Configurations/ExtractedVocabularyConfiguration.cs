using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Feedback.Infrastructure.Persistence.Configurations;

internal sealed class ExtractedVocabularyConfiguration : IEntityTypeConfiguration<ExtractedVocabulary>
{
    public void Configure(EntityTypeBuilder<ExtractedVocabulary> b)
    {
        b.ToTable("extracted_vocabulary");
        b.HasKey(v => v.Id);

        b.Property(v => v.Id).HasColumnName("id");
        b.Property(v => v.SessionFeedbackId).HasColumnName("session_feedback_id").IsRequired();
        b.Property(v => v.Term).HasColumnName("term").HasMaxLength(255).IsRequired();
        b.Property(v => v.Meaning).HasColumnName("meaning").IsRequired();
        b.Property(v => v.ExampleSentence).HasColumnName("example_sentence");
        b.Property(v => v.Difficulty).HasColumnName("difficulty").HasMaxLength(50);
        b.Property(v => v.Confidence).HasColumnName("confidence").IsRequired();

        // Audit columns:
        b.Property(v => v.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(v => v.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(v => v.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(v => v.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
