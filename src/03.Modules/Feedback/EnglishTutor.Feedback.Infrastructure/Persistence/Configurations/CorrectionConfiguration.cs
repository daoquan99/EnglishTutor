using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Feedback.Infrastructure.Persistence.Configurations;

internal sealed class CorrectionConfiguration : IEntityTypeConfiguration<Correction>
{
    public void Configure(EntityTypeBuilder<Correction> b)
    {
        b.ToTable("corrections");
        b.HasKey(c => c.Id);

        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.SessionFeedbackId).HasColumnName("session_feedback_id").IsRequired();
        b.Property(c => c.OriginalText).HasColumnName("original_text").IsRequired();
        b.Property(c => c.CorrectedText).HasColumnName("corrected_text").IsRequired();
        b.Property(c => c.Explanation).HasColumnName("explanation").IsRequired();
        b.Property(c => c.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        b.Property(c => c.Severity).HasColumnName("severity").HasMaxLength(50).IsRequired();

        // Audit columns:
        b.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(c => c.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(c => c.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(c => c.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
