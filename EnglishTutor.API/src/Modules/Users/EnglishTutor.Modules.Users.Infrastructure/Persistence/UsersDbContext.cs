using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Domain.Entities;
using EnglishTutor.Modules.Users.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence;

public sealed class UsersDbContext(
    DbContextOptions<UsersDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IUsersUnitOfWork
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserLanguageSettings> UserLanguageSettings => Set<UserLanguageSettings>();
    public DbSet<UserTargetLanguage> UserTargetLanguages => Set<UserTargetLanguage>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<UserProfile>(builder =>
        {
            builder.ToTable("UserProfiles");
            builder.HasKey(profile => profile.Id);
            builder.Property(profile => profile.DisplayName)
                .HasConversion(name => name.Value, value => Domain.ValueObjects.DisplayName.Create(value))
                .HasMaxLength(100);
            builder.Property(profile => profile.AvatarUrl).HasMaxLength(2048);
            builder.Property(profile => profile.Bio).HasMaxLength(500);
            builder.HasIndex(profile => profile.UserId).IsUnique();
        });

        modelBuilder.Entity<UserLanguageSettings>(builder =>
        {
            builder.ToTable("UserLanguageSettings");
            builder.HasKey(settings => settings.Id);
            builder.Property(settings => settings.NativeLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(settings => settings.UiLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(settings => settings.ExplanationLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(settings => settings.ActiveTargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(settings => settings.UserId).IsUnique();
        });

        modelBuilder.Entity<UserTargetLanguage>(builder =>
        {
            builder.ToTable("UserTargetLanguages");
            builder.HasKey(language => language.Id);
            builder.Property(language => language.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(language => language.CurrentLevel).HasConversion<string>().HasMaxLength(3);
            builder.Property(language => language.TargetLevel).HasConversion<string>().HasMaxLength(3);
            builder.HasIndex(language => new { language.UserId, language.TargetLanguageCode }).IsUnique();
            builder.HasIndex(language => language.UserId);
        });

        modelBuilder.Entity<UserPreference>(builder =>
        {
            builder.ToTable("UserPreferences");
            builder.HasKey(preference => preference.Id);
            builder.Property(preference => preference.Key).HasMaxLength(100);
            builder.Property(preference => preference.Value).HasMaxLength(1000);
            builder.HasIndex(preference => new { preference.UserId, preference.Key }).IsUnique();
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
                    UserProfileUpdatedDomainEvent updated => new UserProfileUpdatedIntegrationEvent(
                        updated.UserId,
                        updated.DisplayName,
                        updated.AvatarUrl,
                        updated.Bio,
                        updated.OccurredOnUtc)
                    {
                        EventId = updated.EventId,
                        OccurredOnUtc = updated.OccurredOnUtc
                    },
                    UserLanguageSettingsUpdatedDomainEvent updated => new UserLanguageSettingsUpdatedIntegrationEvent(
                        updated.UserId,
                        updated.NativeLanguageCode,
                        updated.UiLanguageCode,
                        updated.ExplanationLanguageCode,
                        updated.ActiveTargetLanguageCode,
                        updated.OccurredOnUtc)
                    {
                        EventId = updated.EventId,
                        OccurredOnUtc = updated.OccurredOnUtc
                    },
                    UserTargetLanguageAddedDomainEvent added => new UserTargetLanguageChangedIntegrationEvent(
                        added.UserId,
                        added.TargetLanguageCode,
                        added.CurrentLevel,
                        added.TargetLevel,
                        added.IsActive,
                        added.OccurredOnUtc)
                    {
                        EventId = added.EventId,
                        OccurredOnUtc = added.OccurredOnUtc
                    },
                    UserTargetLanguageActivationChangedDomainEvent changed => new UserTargetLanguageChangedIntegrationEvent(
                        changed.UserId,
                        changed.TargetLanguageCode,
                        changed.CurrentLevel,
                        changed.TargetLevel,
                        changed.IsActive,
                        changed.ChangedAtUtc)
                    {
                        EventId = changed.EventId,
                        OccurredOnUtc = changed.OccurredOnUtc
                    },
                    UserLevelChangedDomainEvent changed => new UserLevelChangedIntegrationEvent(
                        changed.UserId,
                        changed.TargetLanguageCode,
                        changed.PreviousLevel,
                        changed.NewLevel,
                        changed.ChangedAtUtc)
                    {
                        EventId = changed.EventId,
                        OccurredOnUtc = changed.OccurredOnUtc
                    },
                    _ => null
                };

                if (integrationEvent is null)
                {
                    continue;
                }

                OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent, "users", serializer.Serialize(integrationEvent)));
            }

            holder.ClearDomainEvents();
        }
    }
}
