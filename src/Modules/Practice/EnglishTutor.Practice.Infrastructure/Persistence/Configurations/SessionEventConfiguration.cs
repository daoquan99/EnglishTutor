using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Configurations;

internal sealed class SessionEventConfiguration : IEntityTypeConfiguration<SessionEvent>
{
    public void Configure(EntityTypeBuilder<SessionEvent> b)
    {
        b.ToTable("session_events");
        b.HasKey(e => e.Id);

        b.Property(e => e.Id).HasColumnName("id");
        b.Property(e => e.PracticeSessionId).HasColumnName("practice_session_id").IsRequired();
        b.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        b.Property(e => e.Payload).HasColumnName("payload");

        // Index
        b.HasIndex(e => e.PracticeSessionId).HasDatabaseName("ix_session_events_session_id");

        // Audit columns:
        b.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");
    }
}
