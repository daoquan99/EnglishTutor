using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using EnglishTutor.Modules.Vocabulary.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;

public sealed class VocabularyDbContext(
    DbContextOptions<VocabularyDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IVocabularyUnitOfWork
{
    public DbSet<VocabularyItem> VocabularyItems => Set<VocabularyItem>();
    public DbSet<VocabularyTranslation> VocabularyTranslations => Set<VocabularyTranslation>();
    public DbSet<VocabularyExample> VocabularyExamples => Set<VocabularyExample>();
    public DbSet<VocabularyExampleTranslation> VocabularyExampleTranslations => Set<VocabularyExampleTranslation>();
    public DbSet<UserVocabularyMastery> UserVocabularyMasteries => Set<UserVocabularyMastery>();
    public DbSet<VocabularyReviewSession> VocabularyReviewSessions => Set<VocabularyReviewSession>();
    public DbSet<VocabularyReviewAttempt> VocabularyReviewAttempts => Set<VocabularyReviewAttempt>();
    public DbSet<VocabularyPronunciationAttempt> VocabularyPronunciationAttempts => Set<VocabularyPronunciationAttempt>();
    public DbSet<ExampleSentencePronunciationAttempt> ExampleSentencePronunciationAttempts => Set<ExampleSentencePronunciationAttempt>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("vocabulary");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<VocabularyItem>(builder =>
        {
            builder.ToTable("VocabularyItems");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(item => item.PartOfSpeech).HasConversion<string>().HasMaxLength(50);
            builder.Property(item => item.Level).HasConversion<string>().HasMaxLength(3);
            builder.Property(item => item.Word).HasMaxLength(200);
            builder.Property(item => item.Phonetic).HasMaxLength(200);
            builder.Property(item => item.Topic).HasMaxLength(150);
            builder.HasIndex(item => new { item.TargetLanguageCode, item.Word, item.PartOfSpeech }).IsUnique();
            builder.Metadata.FindNavigation(nameof(VocabularyItem.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(VocabularyItem.Examples))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<VocabularyTranslation>(builder =>
        {
            builder.ToTable("VocabularyTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
        });

        modelBuilder.Entity<VocabularyExample>(builder =>
        {
            builder.ToTable("VocabularyExamples");
            builder.HasKey(example => example.Id);
            builder.Property(example => example.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(example => example.Level).HasConversion<string>().HasMaxLength(3);
            builder.Metadata.FindNavigation(nameof(VocabularyExample.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<VocabularyExampleTranslation>(builder =>
        {
            builder.ToTable("VocabularyExampleTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
        });

        modelBuilder.Entity<UserVocabularyMastery>(builder =>
        {
            builder.ToTable("UserVocabularyMasteries");
            builder.HasKey(mastery => mastery.Id);
            builder.Property(mastery => mastery.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(mastery => mastery.Status).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(mastery => new { mastery.UserId, mastery.VocabularyItemId, mastery.TargetLanguageCode }).IsUnique();
            builder.HasIndex(mastery => new { mastery.UserId, mastery.NextReviewAtUtc });
        });

        modelBuilder.Entity<VocabularyReviewSession>(builder =>
        {
            builder.ToTable("VocabularyReviewSessions");
            builder.HasKey(session => session.Id);
            builder.Property(session => session.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
        });

        modelBuilder.Entity<VocabularyReviewAttempt>(builder =>
        {
            builder.ToTable("VocabularyReviewAttempts");
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
        });

        modelBuilder.Entity<VocabularyPronunciationAttempt>(builder =>
        {
            builder.ToTable("VocabularyPronunciationAttempts");
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(attempt => new { attempt.UserId, attempt.VocabularyItemId });
        });

        modelBuilder.Entity<ExampleSentencePronunciationAttempt>(builder =>
        {
            builder.ToTable("ExampleSentencePronunciationAttempts");
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.SourceModule).HasMaxLength(100).IsRequired();
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(message => message.Payload).IsRequired();
            builder.HasIndex(message => new { message.Status, message.NextRetryAtUtc });
        });

        modelBuilder.ApplySoftDeleteQueryFilters();
    }

    public override int SaveChanges()
    {
        AddOutboxMessages();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AddOutboxMessages();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AddOutboxMessages()
    {
        var holders = ChangeTracker.Entries<IDomainEventHolder>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var holder in holders)
        {
            foreach (var domainEvent in holder.DomainEvents)
            {
                IntegrationEvent? integrationEvent = domainEvent switch
                {
                    VocabularyReviewedDomainEvent reviewed => new VocabularyReviewedIntegrationEvent(
                        reviewed.UserId,
                        reviewed.VocabularyItemId,
                        reviewed.TargetLanguageCode,
                        reviewed.IsCorrect,
                        reviewed.Score,
                        reviewed.MasteryStatus,
                        reviewed.ReviewedAtUtc)
                    {
                        EventId = reviewed.EventId,
                        OccurredOnUtc = reviewed.OccurredOnUtc
                    },
                    VocabularyMasteredDomainEvent mastered => new VocabularyMasteredIntegrationEvent(
                        mastered.UserId,
                        mastered.VocabularyItemId,
                        mastered.TargetLanguageCode,
                        mastered.MasteredAtUtc)
                    {
                        EventId = mastered.EventId,
                        OccurredOnUtc = mastered.OccurredOnUtc
                    },
                    VocabularyPronunciationPracticedDomainEvent practiced => new VocabularyPronunciationPracticedIntegrationEvent(
                        practiced.UserId,
                        practiced.VocabularyItemId,
                        practiced.TargetLanguageCode,
                        practiced.PronunciationScore,
                        practiced.AccuracyScore,
                        practiced.FluencyScore,
                        practiced.PracticedAtUtc)
                    {
                        EventId = practiced.EventId,
                        OccurredOnUtc = practiced.OccurredOnUtc
                    },
                    ExampleSentencePronunciationPracticedDomainEvent practiced => new ExampleSentencePronunciationPracticedIntegrationEvent(
                        practiced.UserId,
                        practiced.VocabularyExampleId,
                        practiced.TargetLanguageCode,
                        practiced.PronunciationScore,
                        practiced.AccuracyScore,
                        practiced.FluencyScore,
                        practiced.PracticedAtUtc)
                    {
                        EventId = practiced.EventId,
                        OccurredOnUtc = practiced.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is null)
                {
                    continue;
                }

                OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "vocabulary", serializer.Serialize(integrationEvent)));
            }

            holder.ClearDomainEvents();
        }
    }
}
