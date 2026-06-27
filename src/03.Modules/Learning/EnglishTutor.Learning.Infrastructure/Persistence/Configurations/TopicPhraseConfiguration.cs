using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class TopicPhraseConfiguration : IEntityTypeConfiguration<TopicPhrase>
{
    public void Configure(EntityTypeBuilder<TopicPhrase> b)
    {
        b.ToTable("topic_phrases");
        b.HasKey(p => p.Id);

        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.TopicId).HasColumnName("topic_id").IsRequired();
        b.Property(p => p.Phrase).HasColumnName("phrase").HasMaxLength(500).IsRequired();
        b.Property(p => p.PhraseNormalized).HasColumnName("phrase_normalized").HasMaxLength(500).IsRequired();
        b.Property(p => p.Translation).HasColumnName("translation").HasMaxLength(1000).IsRequired();
        b.Property(p => p.Context).HasColumnName("context").HasMaxLength(1000);
        b.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        // Audit columns
        b.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(p => p.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(p => p.UpdatedByUserId).HasColumnName("updated_by_user_id");

        // Soft delete columns
        b.Property(p => p.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        b.Property(p => p.DeletedAtUtc).HasColumnName("deleted_at_utc");
        b.Property(p => p.DeletedByUserId).HasColumnName("deleted_by_user_id");

        b.Ignore(p => p.DomainEvents);

        // Cascade delete relationship
        b.HasOne<Domain.Aggregates.Topics.Topic>()
            .WithMany()
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique Composite Filter Index on phrase_normalized + topic_id
        b.HasIndex(p => new { p.TopicId, p.PhraseNormalized })
            .HasDatabaseName("ix_topic_phrases_topic_id_phrase_normalized")
            .IsUnique()
            .HasFilter("is_deleted = false");

        // Index on topic_id for joins
        b.HasIndex(p => p.TopicId).HasDatabaseName("ix_topic_phrases_topic_id");
    }
}
