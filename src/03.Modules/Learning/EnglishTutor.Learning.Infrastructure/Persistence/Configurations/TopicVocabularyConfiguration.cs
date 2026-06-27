using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class TopicVocabularyConfiguration : IEntityTypeConfiguration<TopicVocabulary>
{
    public void Configure(EntityTypeBuilder<TopicVocabulary> b)
    {
        b.ToTable("topic_vocabularies");
        b.HasKey(v => v.Id);

        b.Property(v => v.Id).HasColumnName("id");
        b.Property(v => v.TopicId).HasColumnName("topic_id").IsRequired();
        b.Property(v => v.Word).HasColumnName("word").HasMaxLength(255).IsRequired();
        b.Property(v => v.WordNormalized).HasColumnName("word_normalized").HasMaxLength(255).IsRequired();
        b.Property(v => v.Definition).HasColumnName("definition").HasMaxLength(1000).IsRequired();
        b.Property(v => v.PartOfSpeech).HasColumnName("part_of_speech").HasMaxLength(50);
        b.Property(v => v.Phonetic).HasColumnName("phonetic").HasMaxLength(100);
        b.Property(v => v.ExampleSentence).HasColumnName("example_sentence").HasMaxLength(1000);
        b.Property(v => v.ExampleTranslation).HasColumnName("example_translation").HasMaxLength(1000);
        b.Property(v => v.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        // Audit columns
        b.Property(v => v.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(v => v.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(v => v.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(v => v.UpdatedByUserId).HasColumnName("updated_by_user_id");

        // Soft delete columns
        b.Property(v => v.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        b.Property(v => v.DeletedAtUtc).HasColumnName("deleted_at_utc");
        b.Property(v => v.DeletedByUserId).HasColumnName("deleted_by_user_id");

        b.Ignore(v => v.DomainEvents);

        // Cascade delete relationship
        b.HasOne<Domain.Aggregates.Topics.Topic>()
            .WithMany()
            .HasForeignKey(v => v.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique Composite Filter Index on word_normalized + topic_id
        b.HasIndex(v => new { v.TopicId, v.WordNormalized })
            .HasDatabaseName("ix_topic_vocabularies_topic_id_word_normalized")
            .IsUnique()
            .HasFilter("is_deleted = false");

        // Index on topic_id for joins
        b.HasIndex(v => v.TopicId).HasDatabaseName("ix_topic_vocabularies_topic_id");
    }
}
