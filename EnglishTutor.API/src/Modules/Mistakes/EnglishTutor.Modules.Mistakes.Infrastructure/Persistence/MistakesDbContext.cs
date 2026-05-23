using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;

public sealed class MistakesDbContext(
    DbContextOptions<MistakesDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IMistakesUnitOfWork
{
    public DbSet<Mistake> Mistakes => Set<Mistake>();
    public DbSet<MistakeReview> MistakeReviews => Set<MistakeReview>();
    public DbSet<MistakeCategory> MistakeCategories => Set<MistakeCategory>();
    public DbSet<UserMistakeCard> UserMistakeCards => Set<UserMistakeCard>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mistakes");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<Mistake>(builder =>
        {
            builder.ToTable("Mistakes");
            builder.HasKey(mistake => mistake.Id);
            builder.Property(mistake => mistake.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(mistake => mistake.NativeLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(mistake => mistake.ExplanationLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(mistake => mistake.Type).HasConversion<string>().HasMaxLength(50);
            builder.Property(mistake => mistake.SourceType).HasConversion<string>().HasMaxLength(100);
            builder.Property(mistake => mistake.Status).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(mistake => new { mistake.UserId, mistake.NextReviewAtUtc });
        });

        modelBuilder.Entity<MistakeReview>(builder =>
        {
            builder.ToTable("MistakeReviews");
            builder.HasKey(review => review.Id);
        });

        modelBuilder.Entity<MistakeCategory>(builder =>
        {
            builder.ToTable("MistakeCategories");
            builder.HasKey(category => category.Id);
            builder.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<UserMistakeCard>(builder =>
        {
            builder.ToTable("UserMistakeCards");
            builder.HasKey(card => card.Id);
            builder.Property(card => card.TargetLanguageCode)
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

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000);
            builder.Property(message => message.HandlerName).HasMaxLength(300);
            builder.HasIndex(message => new { message.EventId, message.HandlerName }).IsUnique();
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
                    MistakeCreatedDomainEvent created => new MistakeCreatedIntegrationEvent(
                        created.UserId,
                        created.MistakeId,
                        created.TargetLanguageCode,
                        created.Type,
                        created.SourceType,
                        created.SourceId,
                        created.CreatedAtUtc)
                    {
                        EventId = created.EventId,
                        OccurredOnUtc = created.OccurredOnUtc
                    },
                    MistakeReviewedDomainEvent reviewed => new MistakeReviewedIntegrationEvent(
                        reviewed.UserId,
                        reviewed.MistakeId,
                        reviewed.TargetLanguageCode,
                        reviewed.ReviewedAtUtc)
                    {
                        EventId = reviewed.EventId,
                        OccurredOnUtc = reviewed.OccurredOnUtc
                    },
                    MistakeMasteredDomainEvent mastered => new MistakeMasteredIntegrationEvent(
                        mastered.UserId,
                        mastered.MistakeId,
                        mastered.TargetLanguageCode,
                        mastered.MasteredAtUtc)
                    {
                        EventId = mastered.EventId,
                        OccurredOnUtc = mastered.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is null)
                {
                    continue;
                }

                OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "mistakes", serializer.Serialize(integrationEvent)));
            }

            holder.ClearDomainEvents();
        }
    }
}
