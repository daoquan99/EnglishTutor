using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext(
    DbContextOptions<AuthDbContext> options,
    JsonSerializerService serializer)
    : DbContext(options), IAuthUnitOfWork
{
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuthSecurityEvent> AuthSecurityEvents => Set<AuthSecurityEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<AuthUser>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Email)
                .HasConversion(email => email.Value, value => Domain.ValueObjects.Email.Create(value))
                .HasMaxLength(256)
                .IsRequired();
            builder.HasIndex(user => user.Email).IsUnique();
            builder.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserCredential>(builder =>
        {
            builder.ToTable("UserCredentials");
            builder.HasKey(credential => credential.Id);
            builder.Property(credential => credential.HashedPassword)
                .HasConversion(password => password.Value, value => Domain.ValueObjects.HashedPassword.Create(value))
                .HasMaxLength(200)
                .IsRequired();
            builder.HasIndex(credential => credential.AuthUserId).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(token => token.Id);
            builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            builder.HasIndex(token => token.AuthUserId);
            builder.HasIndex(token => token.SessionId);
            builder.HasIndex(token => token.TokenHash).IsUnique();
        });

        modelBuilder.Entity<AuthSession>(builder =>
        {
            builder.ToTable("AuthSessions");
            builder.HasKey(session => session.Id);
            builder.Property(session => session.DeviceId).HasMaxLength(128).IsRequired();
            builder.Property(session => session.UserAgent).HasMaxLength(512);
            builder.Property(session => session.IpAddress).HasMaxLength(64);
            builder.Property(session => session.SuspiciousReason).HasMaxLength(300);
            builder.HasIndex(session => session.AuthUserId);
            builder.HasIndex(session => new { session.AuthUserId, session.DeviceId });
        });

        modelBuilder.Entity<AuthSecurityEvent>(builder =>
        {
            builder.ToTable("AuthSecurityEvents");
            builder.HasKey(securityEvent => securityEvent.Id);
            builder.Property(securityEvent => securityEvent.DeviceId).HasMaxLength(128);
            builder.Property(securityEvent => securityEvent.EventType).HasConversion<string>().HasMaxLength(100);
            builder.Property(securityEvent => securityEvent.Severity).HasConversion<string>().HasMaxLength(50);
            builder.Property(securityEvent => securityEvent.IpAddress).HasMaxLength(64);
            builder.Property(securityEvent => securityEvent.UserAgent).HasMaxLength(512);
            builder.Property(securityEvent => securityEvent.MetadataJson).HasMaxLength(4000);
            builder.Property(securityEvent => securityEvent.ReviewNote).HasMaxLength(1000);
            builder.HasIndex(securityEvent => new { securityEvent.AuthUserId, securityEvent.OccurredAtUtc });
            builder.HasIndex(securityEvent => securityEvent.SessionId);
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
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddOutboxMessages()
    {
        var domainEventHolders = ChangeTracker
            .Entries<IDomainEventHolder>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var holder in domainEventHolders)
        {
            foreach (var domainEvent in holder.DomainEvents)
            {
                if (domainEvent is UserRegisteredDomainEvent userRegistered)
                {
                    var integrationEvent = new UserRegisteredIntegrationEvent(
                        userRegistered.UserId,
                        userRegistered.Email,
                        userRegistered.DisplayName,
                        userRegistered.OccurredOnUtc)
                    {
                        EventId = userRegistered.EventId,
                        OccurredOnUtc = userRegistered.OccurredOnUtc
                    };

                    OutboxMessages.Add(new OutboxMessage
                    {
                        EventId = integrationEvent.EventId,
                        EventType = integrationEvent.GetType().AssemblyQualifiedName!,
                        Payload = serializer.Serialize(integrationEvent),
                        SourceModule = "auth"
                    });
                }
            }

            holder.ClearDomainEvents();
        }
    }
}
