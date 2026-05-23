using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext(
    DbContextOptions<AuthDbContext> options,
    IAuthDomainEventToOutboxMapper outboxMapper)
    : DbContext(options), IAuthUnitOfWork
{
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuthSecurityEvent> AuthSecurityEvents => Set<AuthSecurityEvent>();
    public DbSet<AuthRole> AuthRoles => Set<AuthRole>();
    public DbSet<AuthPermission> AuthPermissions => Set<AuthPermission>();
    public DbSet<AuthRolePermission> AuthRolePermissions => Set<AuthRolePermission>();
    public DbSet<AuthUserRole> AuthUserRoles => Set<AuthUserRole>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<AuthUser>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Email)
                .HasConversion(email => email.Value, value => Domain.AuthUser.ValueObjects.Email.Create(value))
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
                .HasConversion(password => password.Value, value => Domain.AuthUser.ValueObjects.HashedPassword.Create(value))
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

        modelBuilder.Entity<AuthRole>(builder =>
        {
            builder.ToTable("Roles");
            builder.HasKey(role => role.Id);
            builder.Property(role => role.Name).HasMaxLength(100).IsRequired();
            builder.Property(role => role.Description).HasMaxLength(300).IsRequired();
            builder.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<AuthPermission>(builder =>
        {
            builder.ToTable("Permissions");
            builder.HasKey(permission => permission.Id);
            builder.Property(permission => permission.Code).HasMaxLength(150).IsRequired();
            builder.Property(permission => permission.Description).HasMaxLength(300).IsRequired();
            builder.HasIndex(permission => permission.Code).IsUnique();
        });

        modelBuilder.Entity<AuthRolePermission>(builder =>
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(rolePermission => rolePermission.Id);
            builder.HasOne<AuthRole>()
                .WithMany()
                .HasForeignKey(rolePermission => rolePermission.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<AuthPermission>()
                .WithMany()
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId }).IsUnique();
            builder.HasIndex(rolePermission => rolePermission.PermissionId);
        });

        modelBuilder.Entity<AuthUserRole>(builder =>
        {
            builder.ToTable("UserRoles");
            builder.HasKey(userRole => userRole.Id);
            builder.HasOne<AuthUser>()
                .WithMany()
                .HasForeignKey(userRole => userRole.AuthUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<AuthRole>()
                .WithMany()
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(userRole => new { userRole.AuthUserId, userRole.RoleId }).IsUnique();
            builder.HasIndex(userRole => userRole.RoleId);
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
        var domainEventHolders = ChangeTracker
            .Entries<IDomainEventHolder>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var holder in domainEventHolders)
        {
            foreach (var domainEvent in holder.DomainEvents)
            {
                var outboxMessage = outboxMapper.Map(domainEvent);
                if (outboxMessage is not null)
                {
                    OutboxMessages.Add(outboxMessage);
                }
            }

            holder.ClearDomainEvents();
        }
    }
}
