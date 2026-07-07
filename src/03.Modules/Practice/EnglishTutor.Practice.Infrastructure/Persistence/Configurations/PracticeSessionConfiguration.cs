using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Configurations;

internal sealed class PracticeSessionConfiguration : IEntityTypeConfiguration<PracticeSession>
{
    public void Configure(EntityTypeBuilder<PracticeSession> b)
    {
        b.ToTable("practice_sessions");
        b.HasKey(s => s.Id);

        b.Property(s => s.Id).HasColumnName("id");
        b.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        b.Property(s => s.QuotaReservationId).HasColumnName("quota_reservation_id").IsRequired();
        b.Property(s => s.RouteLeaseId).HasColumnName("route_lease_id").IsRequired();
        b.Property(s => s.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        b.Property(s => s.EndReason).HasColumnName("end_reason").HasConversion<string>().HasMaxLength(50);
        b.Property(s => s.StartedAtUtc).HasColumnName("started_at_utc").IsRequired();
        b.Property(s => s.EndedAtUtc).HasColumnName("ended_at_utc");
        b.Property(s => s.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();

        // Indexes
        b.HasIndex(s => s.UserId).HasDatabaseName("ix_practice_sessions_user_id");
        b.HasIndex(s => new { s.UserId, s.StartedAtUtc }).HasDatabaseName("ix_practice_sessions_user_id_started_at");

        // Value Object: ScenarioSnapshot
        b.OwnsOne(s => s.ScenarioSnapshot, ss =>
        {
            ss.Property(x => x.ScenarioId).HasColumnName("scenario_id").IsRequired();
            ss.Property(x => x.TopicId).HasColumnName("topic_id").IsRequired();
            ss.Property(x => x.TopicCode).HasColumnName("topic_code").HasMaxLength(100).IsRequired();
            ss.Property(x => x.TopicTitle).HasColumnName("topic_title").HasMaxLength(255).IsRequired();
            ss.Property(x => x.ModeDefinitionId).HasColumnName("mode_definition_id").IsRequired();
            ss.Property(x => x.ModeCode).HasColumnName("mode_code").HasMaxLength(50).IsRequired();
            ss.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            ss.Property(x => x.LearnerFacingInstructions).HasColumnName("learner_facing_instructions").IsRequired();
            ss.WithOwner();
        });
        b.Navigation(s => s.ScenarioSnapshot).IsRequired();

        b.OwnsOne(s => s.LanguageSnapshot, language =>
        {
            language.Property(x => x.LanguagePairId).HasColumnName("language_pair_id").IsRequired();
            language.Property(x => x.NativeLanguageCode).HasColumnName("native_language_code").HasMaxLength(35).IsRequired();
            language.Property(x => x.TargetLanguageCode).HasColumnName("target_language_code").HasMaxLength(35).IsRequired();
            language.Property(x => x.ExplanationLanguageCode).HasColumnName("explanation_language_code").HasMaxLength(35).IsRequired();
            language.Property(x => x.LanguagePairVersion).HasColumnName("language_pair_version").IsRequired();
            language.WithOwner();
        });
        b.Navigation(s => s.LanguageSnapshot).IsRequired();

        // Collections
        b.HasMany(s => s.TranscriptMessages)
            .WithOne()
            .HasForeignKey(t => t.PracticeSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(s => s.SessionEvents)
            .WithOne()
            .HasForeignKey(e => e.PracticeSessionId)
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
