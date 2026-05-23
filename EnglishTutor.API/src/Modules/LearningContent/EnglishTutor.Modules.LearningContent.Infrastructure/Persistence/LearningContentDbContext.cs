using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Entities;
using EnglishTutor.Modules.LearningContent.Domain.Lesson;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Events;
using EnglishTutor.Modules.LearningContent.Domain.Quiz;
using EnglishTutor.Modules.LearningContent.Domain.SentencePattern;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;

public sealed class LearningContentDbContext(
    DbContextOptions<LearningContentDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), ILearningContentUnitOfWork
{
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonTranslation> LessonTranslations => Set<LessonTranslation>();
    public DbSet<LessonSection> LessonSections => Set<LessonSection>();
    public DbSet<LessonSectionTranslation> LessonSectionTranslations => Set<LessonSectionTranslation>();
    public DbSet<ConversationScenario> ConversationScenarios => Set<ConversationScenario>();
    public DbSet<ConversationScenarioTranslation> ConversationScenarioTranslations => Set<ConversationScenarioTranslation>();
    public DbSet<ConversationLine> ConversationLines => Set<ConversationLine>();
    public DbSet<ConversationLineTranslation> ConversationLineTranslations => Set<ConversationLineTranslation>();
    public DbSet<SentencePattern> SentencePatterns => Set<SentencePattern>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<UserLearningPathCard> UserLearningPathCards => Set<UserLearningPathCard>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("learningcontent");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<Lesson>(builder =>
        {
            builder.ToTable("Lessons");
            builder.HasKey(lesson => lesson.Id);
            builder.Property(lesson => lesson.TargetLanguageCode).HasMaxLength(3);
            builder.Property(lesson => lesson.Level).HasConversion<string>().HasMaxLength(3);
            builder.Property(lesson => lesson.Skill).HasConversion<string>().HasMaxLength(50);
            builder.Property(lesson => lesson.Topic).HasMaxLength(100);
            builder.Property(lesson => lesson.Title).HasMaxLength(200);
            builder.Property(lesson => lesson.Description).HasMaxLength(1000);
            builder.HasIndex(lesson => new { lesson.TargetLanguageCode, lesson.Level, lesson.IsPublished });
            builder.HasIndex(lesson => new { lesson.Topic, lesson.Skill });
            builder.Metadata.FindNavigation(nameof(Lesson.Sections))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(Lesson.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<LessonTranslation>(builder =>
        {
            builder.ToTable("LessonTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode).HasMaxLength(3);
            builder.Property(translation => translation.Title).HasMaxLength(200);
            builder.Property(translation => translation.Description).HasMaxLength(1000);
            builder.HasOne<Lesson>().WithMany(lesson => lesson.Translations).HasForeignKey(translation => translation.LessonId);
            builder.HasIndex(translation => new { translation.LessonId, translation.LanguageCode }).IsUnique();
        });

        modelBuilder.Entity<LessonSection>(builder =>
        {
            builder.ToTable("LessonSections");
            builder.HasKey(section => section.Id);
            builder.Property(section => section.Title).HasMaxLength(200);
            builder.Property(section => section.Content).HasColumnType("text");
            builder.Property(section => section.SectionType).HasMaxLength(50);
            builder.HasOne<Lesson>().WithMany(lesson => lesson.Sections).HasForeignKey(section => section.LessonId);
            builder.HasIndex(section => new { section.LessonId, section.Order }).IsUnique();
            builder.Metadata.FindNavigation(nameof(LessonSection.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<LessonSectionTranslation>(builder =>
        {
            builder.ToTable("LessonSectionTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode).HasMaxLength(3);
            builder.Property(translation => translation.Title).HasMaxLength(200);
            builder.Property(translation => translation.Content).HasColumnType("text");
            builder.HasOne<LessonSection>().WithMany(section => section.Translations).HasForeignKey(translation => translation.LessonSectionId);
            builder.HasIndex(translation => new { translation.LessonSectionId, translation.LanguageCode }).IsUnique();
        });

        modelBuilder.Entity<ConversationScenario>(builder =>
        {
            builder.ToTable("ConversationScenarios");
            builder.HasKey(scenario => scenario.Id);
            builder.Property(scenario => scenario.TargetLanguageCode).HasMaxLength(3);
            builder.Property(scenario => scenario.Level).HasConversion<string>().HasMaxLength(3);
            builder.Property(scenario => scenario.Title).HasMaxLength(200);
            builder.Property(scenario => scenario.Description).HasMaxLength(1000);
            builder.Property(scenario => scenario.Setting).HasMaxLength(200);
            builder.HasIndex(scenario => new { scenario.TargetLanguageCode, scenario.Level, scenario.IsPublished });
            builder.Metadata.FindNavigation(nameof(ConversationScenario.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(ConversationScenario.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ConversationScenarioTranslation>(builder =>
        {
            builder.ToTable("ConversationScenarioTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode).HasMaxLength(3);
            builder.Property(translation => translation.Title).HasMaxLength(200);
            builder.Property(translation => translation.Description).HasMaxLength(1000);
            builder.Property(translation => translation.Setting).HasMaxLength(200);
            builder.HasOne<ConversationScenario>().WithMany(scenario => scenario.Translations).HasForeignKey(translation => translation.ScenarioId);
            builder.HasIndex(translation => new { translation.ScenarioId, translation.LanguageCode }).IsUnique();
        });

        modelBuilder.Entity<ConversationLine>(builder =>
        {
            builder.ToTable("ConversationLines");
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Speaker).HasConversion<string>().HasMaxLength(20);
            builder.Property(line => line.Text).HasMaxLength(2000);
            builder.Property(line => line.ExpectedResponseHint).HasMaxLength(1000);
            builder.Property(line => line.AudioUrl).HasMaxLength(2048);
            builder.Property(line => line.Notes).HasMaxLength(1000);
            builder.HasOne<ConversationScenario>().WithMany(scenario => scenario.Lines).HasForeignKey(line => line.ScenarioId);
            builder.HasIndex(line => new { line.ScenarioId, line.Order }).IsUnique();
            builder.Metadata.FindNavigation(nameof(ConversationLine.Translations))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ConversationLineTranslation>(builder =>
        {
            builder.ToTable("ConversationLineTranslations");
            builder.HasKey(translation => translation.Id);
            builder.Property(translation => translation.LanguageCode).HasMaxLength(3);
            builder.Property(translation => translation.Text).HasMaxLength(2000);
            builder.Property(translation => translation.ExpectedResponseHint).HasMaxLength(1000);
            builder.HasOne<ConversationLine>().WithMany(line => line.Translations).HasForeignKey(translation => translation.ConversationLineId);
            builder.HasIndex(translation => new { translation.ConversationLineId, translation.LanguageCode }).IsUnique();
        });

        modelBuilder.Entity<SentencePattern>(builder =>
        {
            builder.ToTable("SentencePatterns");
            builder.HasKey(pattern => pattern.Id);
            builder.Property(pattern => pattern.TargetLanguageCode).HasMaxLength(3);
            builder.Property(pattern => pattern.Level).HasConversion<string>().HasMaxLength(3);
            builder.Property(pattern => pattern.Pattern).HasMaxLength(500);
            builder.Property(pattern => pattern.Explanation).HasMaxLength(2000);
            builder.Property(pattern => pattern.Examples).HasColumnType("jsonb");
            builder.Property(pattern => pattern.Topic).HasMaxLength(100);
            builder.HasIndex(pattern => new { pattern.TargetLanguageCode, pattern.Level });
        });

        modelBuilder.Entity<Quiz>(builder =>
        {
            builder.ToTable("Quizzes");
            builder.HasKey(quiz => quiz.Id);
            builder.Property(quiz => quiz.Title).HasMaxLength(200);
            builder.Property(quiz => quiz.QuestionsJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<UserLearningPathCard>(builder =>
        {
            builder.ToTable("UserLearningPathCards");
            builder.HasKey(card => card.Id);
            builder.Property(card => card.TargetLanguageCode).HasMaxLength(3);
            builder.Property(card => card.ContentType).HasConversion<string>().HasMaxLength(50);
            builder.Property(card => card.Title).HasMaxLength(200);
            builder.Property(card => card.Level).HasMaxLength(10);
            builder.Property(card => card.Skill).HasMaxLength(50);
            builder.Property(card => card.Topic).HasMaxLength(100);
            builder.Property(card => card.Status).HasConversion<string>().HasMaxLength(30);
            builder.HasIndex(card => new { card.UserId, card.TargetLanguageCode, card.Order });
            builder.HasIndex(card => new { card.UserId, card.ContentType, card.ContentId }).IsUnique();
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
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.HandlerName).HasMaxLength(300).IsRequired();
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
                    LessonPublishedDomainEvent published => new LessonPublishedIntegrationEvent(
                        published.LessonId,
                        published.TargetLanguageCode,
                        published.Level,
                        published.Topic,
                        published.Skill,
                        published.PublishedAtUtc)
                    {
                        EventId = published.EventId,
                        OccurredOnUtc = published.OccurredOnUtc
                    },
                    LessonCompletedDomainEvent completed => new LessonCompletedIntegrationEvent(
                        completed.UserId,
                        completed.LessonId,
                        completed.TargetLanguageCode,
                        completed.Level,
                        completed.Topic,
                        completed.Skill,
                        completed.DurationSeconds,
                        completed.CompletedAtUtc)
                    {
                        EventId = completed.EventId,
                        OccurredOnUtc = completed.OccurredOnUtc
                    },
                    ConversationScenarioCompletedDomainEvent completed => new ConversationScenarioCompletedIntegrationEvent(
                        completed.UserId,
                        completed.ConversationScenarioId,
                        completed.TargetLanguageCode,
                        completed.Level,
                        completed.DurationSeconds,
                        completed.CompletedAtUtc)
                    {
                        EventId = completed.EventId,
                        OccurredOnUtc = completed.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is not null)
                {
                    OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "learningcontent", serializer.Serialize(integrationEvent)));
                }
            }

            holder.ClearDomainEvents();
        }
    }
}
