using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Persistence;

public sealed class ExercisesDbContext(
    DbContextOptions<ExercisesDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IExercisesUnitOfWork
{
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public DbSet<ExerciseQuestion> ExerciseQuestions => Set<ExerciseQuestion>();
    public DbSet<ExerciseOption> ExerciseOptions => Set<ExerciseOption>();
    public DbSet<UserExerciseAttempt> UserExerciseAttempts => Set<UserExerciseAttempt>();
    public DbSet<UserExerciseAnswer> UserExerciseAnswers => Set<UserExerciseAnswer>();
    public DbSet<UserExerciseResult> UserExerciseResults => Set<UserExerciseResult>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("exercises");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<ExerciseSet>(builder =>
        {
            builder.ToTable("ExerciseSets");
            builder.HasKey(exerciseSet => exerciseSet.Id);
            builder.Property(exerciseSet => exerciseSet.TargetLanguageCode).HasMaxLength(3);
            builder.Property(exerciseSet => exerciseSet.Level).HasConversion<string>().HasMaxLength(3);
            builder.Property(exerciseSet => exerciseSet.Topic).HasMaxLength(100);
            builder.Property(exerciseSet => exerciseSet.Skill).HasConversion<string>().HasMaxLength(50);
            builder.Property(exerciseSet => exerciseSet.ExerciseType).HasConversion<string>().HasMaxLength(50);
            builder.Property(exerciseSet => exerciseSet.Title).HasMaxLength(200);
            builder.Property(exerciseSet => exerciseSet.Description).HasMaxLength(1000);
            builder.HasIndex(exerciseSet => new { exerciseSet.TargetLanguageCode, exerciseSet.Level, exerciseSet.ExerciseType, exerciseSet.IsPublished });
            builder.Metadata.FindNavigation(nameof(ExerciseSet.Questions))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ExerciseQuestion>(builder =>
        {
            builder.ToTable("ExerciseQuestions");
            builder.HasKey(question => question.Id);
            builder.Property(question => question.QuestionType).HasConversion<string>().HasMaxLength(50);
            builder.Property(question => question.Prompt).HasColumnType("text");
            builder.Property(question => question.CorrectAnswer).HasColumnType("text");
            builder.Property(question => question.Explanation).HasColumnType("text");
            builder.Property(question => question.Difficulty).HasConversion<string>().HasMaxLength(20);
            builder.HasOne<ExerciseSet>().WithMany(exerciseSet => exerciseSet.Questions).HasForeignKey(question => question.ExerciseSetId);
            builder.HasIndex(question => new { question.ExerciseSetId, question.Order }).IsUnique();
            builder.Metadata.FindNavigation(nameof(ExerciseQuestion.Options))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ExerciseOption>(builder =>
        {
            builder.ToTable("ExerciseOptions");
            builder.HasKey(option => option.Id);
            builder.Property(option => option.OptionText).HasMaxLength(1000);
            builder.HasOne<ExerciseQuestion>().WithMany(question => question.Options).HasForeignKey(option => option.QuestionId);
            builder.HasIndex(option => new { option.QuestionId, option.Order }).IsUnique();
        });

        modelBuilder.Entity<UserExerciseAttempt>(builder =>
        {
            builder.ToTable("UserExerciseAttempts");
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.TargetLanguageCode).HasMaxLength(3);
            builder.Property(attempt => attempt.Status).HasConversion<string>().HasMaxLength(30);
            builder.HasIndex(attempt => new { attempt.UserId, attempt.ExerciseSetId });
            builder.HasIndex(attempt => new { attempt.UserId, attempt.Status });
            builder.Metadata.FindNavigation(nameof(UserExerciseAttempt.Answers))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UserExerciseAnswer>(builder =>
        {
            builder.ToTable("UserExerciseAnswers");
            builder.HasKey(answer => answer.Id);
            builder.Property(answer => answer.UserAnswer).HasColumnType("text");
            builder.Property(answer => answer.Feedback).HasColumnType("text");
            builder.HasOne<UserExerciseAttempt>().WithMany(attempt => attempt.Answers).HasForeignKey(answer => answer.AttemptId);
            builder.HasIndex(answer => new { answer.AttemptId, answer.QuestionId }).IsUnique();
        });

        modelBuilder.Entity<UserExerciseResult>(builder =>
        {
            builder.ToTable("UserExerciseResults");
            builder.HasKey(result => result.Id);
            builder.Property(result => result.TargetLanguageCode).HasMaxLength(3);
            builder.Property(result => result.ExerciseType).HasConversion<string>().HasMaxLength(50);
            builder.HasOne<UserExerciseAttempt>().WithOne().HasForeignKey<UserExerciseResult>(result => result.AttemptId);
            builder.HasIndex(result => new { result.UserId, result.TargetLanguageCode });
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
                IntegrationEvent? integrationEvent = domainEvent switch
                {
                    ExerciseStartedDomainEvent started => new ExerciseStartedIntegrationEvent(
                        started.UserId,
                        started.AttemptId,
                        started.ExerciseSetId,
                        started.TargetLanguageCode,
                        started.StartedAtUtc)
                    {
                        EventId = started.EventId,
                        OccurredOnUtc = started.OccurredOnUtc
                    },
                    ExerciseQuestionAnsweredDomainEvent answered => new ExerciseQuestionAnsweredIntegrationEvent(
                        answered.UserId,
                        answered.AttemptId,
                        answered.ExerciseSetId,
                        answered.QuestionId,
                        answered.TargetLanguageCode,
                        answered.IsCorrect,
                        answered.Score,
                        answered.AnsweredAtUtc)
                    {
                        EventId = answered.EventId,
                        OccurredOnUtc = answered.OccurredOnUtc
                    },
                    ExerciseCompletedDomainEvent completed => new ExerciseCompletedIntegrationEvent(
                        completed.UserId,
                        completed.ExerciseSetId,
                        completed.AttemptId,
                        completed.TargetLanguageCode,
                        completed.ExerciseType,
                        completed.Score,
                        completed.CorrectCount,
                        completed.TotalQuestions,
                        completed.TimeTakenSeconds,
                        completed.WrongAnswers.Select(wrongAnswer => new WrongAnswerDetail(
                            wrongAnswer.QuestionId,
                            wrongAnswer.Prompt,
                            wrongAnswer.UserAnswer,
                            wrongAnswer.CorrectAnswer,
                            wrongAnswer.Explanation,
                            wrongAnswer.QuestionType)).ToArray(),
                        completed.CompletedAtUtc)
                    {
                        EventId = completed.EventId,
                        OccurredOnUtc = completed.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is not null)
                {
                    OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "exercises", serializer.Serialize(integrationEvent)));
                }
            }

            holder.ClearDomainEvents();
        }
    }
}
