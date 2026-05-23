using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using EnglishTutor.Modules.StudyPlans.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence;

public sealed class StudyPlansDbContext(
    DbContextOptions<StudyPlansDbContext> options,
    JsonSerializerService serializer,
    IDateTimeProvider dateTimeProvider)
    : DbContext(options), IStudyPlansUnitOfWork
{
    public DbSet<UserStudyPlan> UserStudyPlans => Set<UserStudyPlan>();
    public DbSet<UserStudyWeekDay> UserStudyWeekDays => Set<UserStudyWeekDay>();
    public DbSet<StudyPlanTarget> StudyPlanTargets => Set<StudyPlanTarget>();
    public DbSet<PlannedStudySession> PlannedStudySessions => Set<PlannedStudySession>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("studyplans");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<UserStudyPlan>(builder =>
        {
            builder.ToTable("UserStudyPlans");
            builder.HasKey(plan => plan.Id);
            builder.Property(plan => plan.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3)
                .IsRequired();
            builder.Property(plan => plan.TimeZoneId).HasMaxLength(100).IsRequired();
            builder.HasIndex(plan => new { plan.UserId, plan.TargetLanguageCode })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDeleted\" = false");
            builder.Metadata.FindNavigation(nameof(UserStudyPlan.WeekDays))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Metadata.FindNavigation(nameof(UserStudyPlan.Targets))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UserStudyWeekDay>(builder =>
        {
            builder.ToTable("UserStudyWeekDays");
            builder.HasKey(weekDay => weekDay.Id);
            builder.Property(weekDay => weekDay.DayOfWeek).HasConversion<string>().HasMaxLength(20);
            builder.HasOne<UserStudyPlan>()
                .WithMany(plan => plan.WeekDays)
                .HasForeignKey(weekDay => weekDay.StudyPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(weekDay => new { weekDay.StudyPlanId, weekDay.DayOfWeek }).IsUnique();
        });

        modelBuilder.Entity<StudyPlanTarget>(builder =>
        {
            builder.ToTable("StudyPlanTargets");
            builder.HasKey(target => target.Id);
            builder.Property(target => target.Period).HasConversion<string>().HasMaxLength(20);
            builder.HasOne<UserStudyPlan>()
                .WithMany(plan => plan.Targets)
                .HasForeignKey(target => target.StudyPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(target => new { target.StudyPlanId, target.Period }).IsUnique();
        });

        modelBuilder.Entity<PlannedStudySession>(builder =>
        {
            builder.ToTable("PlannedStudySessions");
            builder.HasKey(session => session.Id);
            builder.Property(session => session.TargetLanguageCode).HasMaxLength(3);
            builder.Property(session => session.Status).HasConversion<string>().HasMaxLength(30);
            builder.HasIndex(session => new { session.UserId, session.ScheduledDateUtc, session.Status });
            builder.HasIndex(session => new { session.Status, session.ScheduledDateUtc });
            builder.HasIndex(session => new { session.StudyPlanId, session.ScheduledDateUtc });
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
        foreach (var entry in ChangeTracker.Entries<PlannedStudySession>().Where(entry => entry.State == EntityState.Added))
        {
            var session = entry.Entity;
            AddIntegrationEvent(new PlannedStudySessionCreatedIntegrationEvent(
                session.UserId,
                session.StudyPlanId,
                session.Id,
                session.TargetLanguageCode,
                session.ScheduledDateUtc,
                session.CreatedAtUtc == default ? dateTimeProvider.UtcNow : session.CreatedAtUtc));
        }

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
                    StudyPlanCreatedDomainEvent created => new StudyPlanCreatedIntegrationEvent(
                        created.UserId,
                        created.StudyPlanId,
                        created.TargetLanguageCode,
                        created.CreatedAtUtc)
                    {
                        EventId = created.EventId,
                        OccurredOnUtc = created.OccurredOnUtc
                    },
                    StudyPlanUpdatedDomainEvent updated => new StudyPlanUpdatedIntegrationEvent(
                        updated.UserId,
                        updated.StudyPlanId,
                        updated.TargetLanguageCode,
                        updated.UpdatedAtUtc)
                    {
                        EventId = updated.EventId,
                        OccurredOnUtc = updated.OccurredOnUtc
                    },
                    PlannedStudySessionMissedDomainEvent missed => new PlannedStudySessionMissedIntegrationEvent(
                        missed.UserId,
                        missed.SessionId,
                        missed.TargetLanguageCode,
                        missed.ScheduledDateUtc,
                        missed.MissedAtUtc)
                    {
                        EventId = missed.EventId,
                        OccurredOnUtc = missed.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is not null)
                {
                    AddIntegrationEvent(integrationEvent);
                }
            }

            holder.ClearDomainEvents();
        }
    }

    private void AddIntegrationEvent(IntegrationEvent integrationEvent)
    {
        OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "studyplans", serializer.Serialize(integrationEvent)));
    }
}
