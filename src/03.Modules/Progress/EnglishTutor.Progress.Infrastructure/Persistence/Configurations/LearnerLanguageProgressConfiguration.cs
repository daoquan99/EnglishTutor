using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Progress.Infrastructure.Persistence.Configurations;

internal sealed class LearnerLanguageProgressConfiguration
    : IEntityTypeConfiguration<LearnerLanguageProgress>
{
    public void Configure(EntityTypeBuilder<LearnerLanguageProgress> builder)
    {
        builder.ToTable("learner_language_progress");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.LanguagePairId).HasColumnName("language_pair_id").IsRequired();
        builder.Property(x => x.NativeLanguageCode).HasColumnName("native_language_code").HasMaxLength(35).IsRequired();
        builder.Property(x => x.TargetLanguageCode).HasColumnName("target_language_code").HasMaxLength(35).IsRequired();
        builder.Property(x => x.SessionsCompleted).HasColumnName("sessions_completed").IsRequired();
        builder.Property(x => x.SpeakingSeconds).HasColumnName("speaking_seconds").IsRequired();
        builder.Property(x => x.FeedbackCount).HasColumnName("feedback_count").IsRequired();
        builder.Property(x => x.LatestScore).HasColumnName("latest_score");
        builder.Property(x => x.CurrentCefrLevel).HasColumnName("current_cefr_level").HasMaxLength(10);
        builder.Property(x => x.LastPracticedAtUtc).HasColumnName("last_practiced_at_utc");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(x => new { x.UserId, x.LanguagePairId }).IsUnique();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");
        builder.Ignore(x => x.DomainEvents);
    }
}
