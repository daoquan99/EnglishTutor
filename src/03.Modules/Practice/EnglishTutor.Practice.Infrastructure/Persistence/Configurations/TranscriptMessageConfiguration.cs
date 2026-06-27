using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Configurations;

internal sealed class TranscriptMessageConfiguration : IEntityTypeConfiguration<TranscriptMessage>
{
    public void Configure(EntityTypeBuilder<TranscriptMessage> b)
    {
        b.ToTable("transcript_messages");
        b.HasKey(t => t.Id);

        b.Property(t => t.Id).HasColumnName("id");
        b.Property(t => t.PracticeSessionId).HasColumnName("practice_session_id").IsRequired();
        b.Property(t => t.SequenceNumber).HasColumnName("sequence_number").IsRequired();
        b.Property(t => t.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
        b.Property(t => t.Content).HasColumnName("content").IsRequired();

        // Index for rapid sorted retrieval
        b.HasIndex(t => new { t.PracticeSessionId, t.SequenceNumber }).HasDatabaseName("ix_transcript_messages_session_seq");

        // Audit columns:
        b.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(t => t.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(t => t.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
