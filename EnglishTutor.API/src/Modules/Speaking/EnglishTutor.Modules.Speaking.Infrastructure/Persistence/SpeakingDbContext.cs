using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Contracts.DTOs;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence;

public sealed class SpeakingDbContext(
    DbContextOptions<SpeakingDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), ISpeakingUnitOfWork
{
    public DbSet<SpeakingSession> SpeakingSessions => Set<SpeakingSession>();
    public DbSet<SpeakingTurn> SpeakingTurns => Set<SpeakingTurn>();
    public DbSet<SpeakingTurnResult> SpeakingTurnResults => Set<SpeakingTurnResult>();
    public DbSet<SpeakingSessionSummary> SpeakingSessionSummaries => Set<SpeakingSessionSummary>();
    public DbSet<ConversationPracticeResult> ConversationPracticeResults => Set<ConversationPracticeResult>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("speaking");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<SpeakingSession>(builder =>
        {
            builder.ToTable("SpeakingSessions");
            builder.HasKey(session => session.Id);
            builder.Property(session => session.SessionType).HasConversion<string>().HasMaxLength(50);
            builder.Property(session => session.Status).HasConversion<string>().HasMaxLength(50);
            builder.Property(session => session.Topic).HasMaxLength(200);
            builder.Property(session => session.ConversationScenarioId);
            builder.Property(session => session.CurrentLineOrder);
            builder.OwnsOne(session => session.LanguageSnapshot, snapshot =>
            {
                snapshot.Property(value => value.NativeLanguageCode)
                    .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                    .HasColumnName("NativeLanguageCodeAtStart")
                    .HasMaxLength(3);
                snapshot.Property(value => value.TargetLanguageCode)
                    .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                    .HasColumnName("TargetLanguageCodeAtStart")
                    .HasMaxLength(3);
                snapshot.Property(value => value.UiLanguageCode)
                    .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                    .HasColumnName("UiLanguageCodeAtStart")
                    .HasMaxLength(3);
                snapshot.Property(value => value.ExplanationLanguageCode)
                    .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                    .HasColumnName("ExplanationLanguageCodeAtStart")
                    .HasMaxLength(3);
                snapshot.Property(value => value.UserLevel)
                    .HasConversion<string>()
                    .HasColumnName("UserLevelAtStart")
                    .HasMaxLength(3);
            });
            builder.Metadata.FindNavigation(nameof(SpeakingSession.Turns))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SpeakingTurn>(builder =>
        {
            builder.ToTable("SpeakingTurns");
            builder.HasKey(turn => turn.Id);
            builder.Property(turn => turn.UserText).HasMaxLength(4000);
            builder.Property(turn => turn.AudioUrl).HasMaxLength(2048);
            builder.Property(turn => turn.Status).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(turn => new { turn.SpeakingSessionId, turn.TurnNumber }).IsUnique();
        });

        modelBuilder.Entity<SpeakingTurnResult>(builder =>
        {
            builder.ToTable("SpeakingTurnResults");
            builder.HasKey(result => result.Id);
            builder.Property(result => result.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(result => result.SpeakingTurnId).IsUnique();
        });

        modelBuilder.Entity<SpeakingSessionSummary>(builder =>
        {
            builder.ToTable("SpeakingSessionSummaries");
            builder.HasKey(summary => summary.Id);
            builder.Property(summary => summary.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(summary => summary.SpeakingSessionId).IsUnique();
        });

        modelBuilder.Entity<ConversationPracticeResult>(builder =>
        {
            builder.ToTable("ConversationPracticeResults");
            builder.HasKey(result => result.Id);
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
                    SpeakingSessionStartedDomainEvent started => new SpeakingSessionStartedIntegrationEvent(
                        started.UserId,
                        started.SessionId,
                        started.SessionType,
                        started.TargetLanguageCode,
                        started.UserLevel)
                    {
                        EventId = started.EventId,
                        OccurredOnUtc = started.OccurredOnUtc
                    },
                    SpeakingTurnCorrectedDomainEvent corrected => new SpeakingTurnCorrectedIntegrationEvent(
                        corrected.UserId,
                        corrected.SessionId,
                        corrected.TurnId,
                        corrected.TargetLanguageCode,
                        corrected.NativeLanguageCode,
                        corrected.ExplanationLanguageCode,
                        corrected.OriginalText,
                        corrected.CorrectedText,
                        corrected.GrammarScore,
                        corrected.VocabularyScore,
                        corrected.OverallScore,
                        corrected.Mistakes
                            .Select(mistake => new SpeakingMistakeDetail(
                                mistake.Type,
                                mistake.Original,
                                mistake.Corrected,
                                mistake.Explanation))
                            .ToList(),
                        corrected.CorrectedAtUtc)
                    {
                        EventId = corrected.EventId,
                        OccurredOnUtc = corrected.OccurredOnUtc
                    },
                    SpeakingSessionCompletedDomainEvent completed => new SpeakingSessionCompletedIntegrationEvent(
                        completed.UserId,
                        completed.SessionId,
                        completed.TargetLanguageCode,
                        completed.TotalTurns,
                        completed.OverallScore,
                        completed.DurationSeconds,
                        completed.ConversationScenarioId,
                        completed.CompletedAtUtc)
                    {
                        EventId = completed.EventId,
                        OccurredOnUtc = completed.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is null)
                {
                    continue;
                }

                OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "speaking", serializer.Serialize(integrationEvent)));
            }

            holder.ClearDomainEvents();
        }
    }
}
