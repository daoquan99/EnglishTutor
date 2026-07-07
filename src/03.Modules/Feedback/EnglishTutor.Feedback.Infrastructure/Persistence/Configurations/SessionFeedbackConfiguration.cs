using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Feedback.Infrastructure.Persistence.Configurations;

internal sealed class SessionFeedbackConfiguration : IEntityTypeConfiguration<SessionFeedback>
{
    public void Configure(EntityTypeBuilder<SessionFeedback> b)
    {
        b.ToTable("session_feedbacks");
        b.HasKey(s => s.Id);

        b.Property(s => s.Id).HasColumnName("id");
        b.Property(s => s.PracticeSessionId).HasColumnName("practice_session_id").IsRequired();
        b.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        b.Property(s => s.Summary).HasColumnName("summary").IsRequired();
        b.Property(s => s.Strengths).HasColumnName("strengths").IsRequired();
        b.Property(s => s.ImprovementAreas).HasColumnName("improvement_areas").IsRequired();
        b.Property(s => s.Score).HasColumnName("score");
        b.Property(s => s.CefrLevel).HasColumnName("cefr_level").HasMaxLength(10);
        b.Property(s => s.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        b.Property(s => s.FailureReasonCode).HasColumnName("failure_reason_code").HasMaxLength(100);
        b.Property(s => s.LanguagePairId).HasColumnName("language_pair_id").IsRequired();
        b.Property(s => s.NativeLanguageCode).HasColumnName("native_language_code").HasMaxLength(35).IsRequired();
        b.Property(s => s.TargetLanguageCode).HasColumnName("target_language_code").HasMaxLength(35).IsRequired();
        b.Property(s => s.ExplanationLanguageCode).HasColumnName("explanation_language_code").HasMaxLength(35).IsRequired();

        // Indexes
        b.HasIndex(s => s.UserId).HasDatabaseName("ix_session_feedbacks_user_id");
        b.HasIndex(s => s.PracticeSessionId).HasDatabaseName("ix_session_feedbacks_practice_session_id");
        b.HasIndex(s => s.Status).HasDatabaseName("ix_session_feedbacks_status");
        b.HasIndex(s => new { s.UserId, s.LanguagePairId, s.CreatedAtUtc })
            .HasDatabaseName("ix_session_feedbacks_user_language_pair_created");

        // Collections
        b.HasMany(s => s.Corrections)
            .WithOne()
            .HasForeignKey(c => c.SessionFeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(s => s.Vocabulary)
            .WithOne()
            .HasForeignKey(v => v.SessionFeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(s => s.MistakePatterns)
            .WithOne()
            .HasForeignKey(m => m.SessionFeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

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
    }
}
