using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Feedback.Infrastructure.Persistence.Configurations;

internal sealed class MistakePatternConfiguration : IEntityTypeConfiguration<MistakePattern>
{
    public void Configure(EntityTypeBuilder<MistakePattern> b)
    {
        b.ToTable("mistake_patterns");
        b.HasKey(m => m.Id);

        b.Property(m => m.Id).HasColumnName("id");
        b.Property(m => m.SessionFeedbackId).HasColumnName("session_feedback_id").IsRequired();
        b.Property(m => m.Pattern).HasColumnName("pattern").HasMaxLength(255).IsRequired();
        b.Property(m => m.Description).HasColumnName("description").IsRequired();
        b.Property(m => m.Frequency).HasColumnName("frequency").IsRequired();

        // Audit columns:
        b.Property(m => m.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(m => m.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(m => m.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(m => m.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
