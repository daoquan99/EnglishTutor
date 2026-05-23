using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Persistence;

public sealed class AssessmentsDbContext(
    DbContextOptions<AssessmentsDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IAssessmentsUnitOfWork
{
    public DbSet<AssessmentDefinition> AssessmentDefinitions => Set<AssessmentDefinition>();
    public DbSet<AssessmentSection> AssessmentSections => Set<AssessmentSection>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<UserAssessmentAttempt> UserAssessmentAttempts => Set<UserAssessmentAttempt>();
    public DbSet<UserAssessmentAnswer> UserAssessmentAnswers => Set<UserAssessmentAnswer>();
    public DbSet<AssessmentGradingResult> AssessmentGradingResults => Set<AssessmentGradingResult>();
    public DbSet<AssessmentRubric> AssessmentRubrics => Set<AssessmentRubric>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("assessments");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<AssessmentDefinition>(builder =>
        {
            builder.ToTable("AssessmentDefinitions");
            builder.HasKey(definition => definition.Id);
            builder.Property(definition => definition.AssessmentType).HasConversion<string>().HasMaxLength(50);
            builder.Property(definition => definition.TargetLanguageCode).HasMaxLength(3);
            builder.Property(definition => definition.ForLevel).HasConversion<string>().HasMaxLength(3);
            builder.Property(definition => definition.Title).HasMaxLength(200);
            builder.Property(definition => definition.Description).HasMaxLength(1000);
            builder.HasIndex(definition => new { definition.AssessmentType, definition.TargetLanguageCode, definition.ForLevel, definition.IsActive });
            builder.Metadata.FindNavigation(nameof(AssessmentDefinition.Sections))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(AssessmentDefinition.Rubrics))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AssessmentSection>(builder =>
        {
            builder.ToTable("AssessmentSections");
            builder.HasKey(section => section.Id);
            builder.Property(section => section.Skill).HasConversion<string>().HasMaxLength(50);
            builder.Property(section => section.Title).HasMaxLength(200);
            builder.HasOne<AssessmentDefinition>().WithMany(definition => definition.Sections).HasForeignKey(section => section.AssessmentDefinitionId);
            builder.HasIndex(section => new { section.AssessmentDefinitionId, section.Order }).IsUnique();
            builder.Metadata.FindNavigation(nameof(AssessmentSection.Questions))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AssessmentQuestion>(builder =>
        {
            builder.ToTable("AssessmentQuestions");
            builder.HasKey(question => question.Id);
            builder.Property(question => question.Prompt).HasColumnType("text");
            builder.Property(question => question.QuestionType).HasMaxLength(100);
            builder.Property(question => question.CorrectAnswer).HasColumnType("text");
            builder.Property(question => question.Explanation).HasColumnType("text");
            builder.HasOne<AssessmentSection>().WithMany(section => section.Questions).HasForeignKey(question => question.SectionId);
            builder.HasIndex(question => new { question.SectionId, question.Order }).IsUnique();
        });

        modelBuilder.Entity<AssessmentRubric>(builder =>
        {
            builder.ToTable("AssessmentRubrics");
            builder.HasKey(rubric => rubric.Id);
            builder.Property(rubric => rubric.Skill).HasConversion<string>().HasMaxLength(50);
            builder.Property(rubric => rubric.Criteria).HasColumnType("text");
            builder.Property(rubric => rubric.ScoringGuide).HasColumnType("text");
            builder.HasOne<AssessmentDefinition>().WithMany(definition => definition.Rubrics).HasForeignKey(rubric => rubric.AssessmentDefinitionId);
            builder.HasIndex(rubric => new { rubric.AssessmentDefinitionId, rubric.Skill }).IsUnique();
        });

        modelBuilder.Entity<UserAssessmentAttempt>(builder =>
        {
            builder.ToTable("UserAssessmentAttempts");
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.TargetLanguageCode).HasMaxLength(3);
            builder.Property(attempt => attempt.CurrentLevel).HasMaxLength(10);
            builder.Property(attempt => attempt.Status).HasConversion<string>().HasMaxLength(30);
            builder.HasIndex(attempt => new { attempt.UserId, attempt.AssessmentDefinitionId });
            builder.HasIndex(attempt => new { attempt.UserId, attempt.Status, attempt.StartedAtUtc });
            builder.Metadata.FindNavigation(nameof(UserAssessmentAttempt.Answers))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UserAssessmentAnswer>(builder =>
        {
            builder.ToTable("UserAssessmentAnswers");
            builder.HasKey(answer => answer.Id);
            builder.Property(answer => answer.UserAnswer).HasColumnType("text");
            builder.Property(answer => answer.Feedback).HasColumnType("text");
            builder.HasOne<UserAssessmentAttempt>().WithMany(attempt => attempt.Answers).HasForeignKey(answer => answer.AttemptId);
            builder.HasIndex(answer => new { answer.AttemptId, answer.QuestionId }).IsUnique();
        });

        modelBuilder.Entity<AssessmentGradingResult>(builder =>
        {
            builder.ToTable("AssessmentGradingResults");
            builder.HasKey(result => result.Id);
            builder.Property(result => result.SectionScoresJson).HasColumnType("jsonb");
            builder.Property(result => result.GradingNotes).HasColumnType("text");
            builder.HasIndex(result => result.AttemptId).IsUnique();
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
                foreach (var integrationEvent in MapIntegrationEvents(domainEvent))
                {
                    OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "assessments", serializer.Serialize(integrationEvent)));
                }
            }

            holder.ClearDomainEvents();
        }
    }

    private static IReadOnlyList<IntegrationEvent> MapIntegrationEvents(DomainEvent domainEvent) =>
        domainEvent switch
        {
            AssessmentStartedDomainEvent started =>
            [
                new AssessmentStartedIntegrationEvent(
                    started.UserId,
                    started.AssessmentAttemptId,
                    started.AssessmentDefinitionId,
                    started.TargetLanguageCode,
                    started.CurrentLevel,
                    started.StartedAtUtc)
                {
                    EventId = started.EventId,
                    OccurredOnUtc = started.OccurredOnUtc
                }
            ],
            AssessmentPassedDomainEvent passed => CreatePassedEvents(passed),
            AssessmentFailedDomainEvent failed => CreateFailedEvents(failed),
            _ => []
        };

    private static IReadOnlyList<IntegrationEvent> CreatePassedEvents(AssessmentPassedDomainEvent passed)
    {
        var nextLevel = TryGetNextLevel(passed.CurrentLevel);
        var events = new List<IntegrationEvent>
        {
            new AssessmentCompletedIntegrationEvent(
                passed.UserId,
                passed.AssessmentAttemptId,
                passed.AssessmentDefinitionId,
                passed.TargetLanguageCode,
                "LevelUpTest",
                passed.TotalScore,
                true,
                passed.PassedAtUtc)
            {
                EventId = Guid.NewGuid(),
                OccurredOnUtc = passed.OccurredOnUtc
            },
            new AssessmentPassedIntegrationEvent(
                passed.UserId,
                passed.AssessmentAttemptId,
                passed.AssessmentDefinitionId,
                passed.TargetLanguageCode,
                passed.TotalScore,
                passed.PassedAtUtc)
            {
                EventId = Guid.NewGuid(),
                OccurredOnUtc = passed.OccurredOnUtc
            }
        };

        if (nextLevel is not null)
        {
            events.Add(new LevelUpApprovedIntegrationEvent(
                passed.UserId,
                passed.TargetLanguageCode,
                passed.CurrentLevel,
                nextLevel,
                passed.TotalScore,
                passed.AssessmentAttemptId,
                passed.PassedAtUtc)
            {
                EventId = Guid.NewGuid(),
                OccurredOnUtc = passed.OccurredOnUtc
            });
        }

        return events;
    }

    private static IReadOnlyList<IntegrationEvent> CreateFailedEvents(AssessmentFailedDomainEvent failed) =>
    [
        new AssessmentCompletedIntegrationEvent(
            failed.UserId,
            failed.AssessmentAttemptId,
            failed.AssessmentDefinitionId,
            failed.TargetLanguageCode,
            "LevelUpTest",
            failed.TotalScore,
            false,
            failed.FailedAtUtc)
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = failed.OccurredOnUtc
        },
        new AssessmentFailedIntegrationEvent(
            failed.UserId,
            failed.AssessmentAttemptId,
            failed.AssessmentDefinitionId,
            failed.TargetLanguageCode,
            failed.TotalScore,
            failed.SectionScores.Where(pair => pair.Value < 60).Select(pair => pair.Key.ToString()).ToArray(),
            failed.FailedAtUtc)
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = failed.OccurredOnUtc
        }
    ];

    private static string? TryGetNextLevel(string currentLevel) =>
        Enum.TryParse<BuildingBlocks.SharedKernel.LanguageLevel>(currentLevel, true, out var level) &&
        level < BuildingBlocks.SharedKernel.LanguageLevel.C2
            ? ((BuildingBlocks.SharedKernel.LanguageLevel)((int)level + 1)).ToString()
            : null;
}
