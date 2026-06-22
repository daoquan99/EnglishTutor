using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public const string SchemaName = "identity";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Soft-delete query filter convention from BuildingBlocks.
        modelBuilder.ApplyAggregateRootConventions();
    }
}

// ===== Configurations =====

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);

        b.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();

        // Email as owned value object.
        b.OwnsOne(u => u.Email, e =>
        {
            e.Property(email => email.Value).HasColumnName("email").HasMaxLength(320).IsRequired();
            e.WithOwner();
            e.HasIndex(email => email.Value).IsUnique();
        });

        // PasswordHash as owned value object (never queryable in plaintext).
        b.OwnsOne(u => u.PasswordHash, p =>
        {
            p.Property(h => h.Hash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            p.WithOwner();
        });

        b.Property(u => u.IsActive).IsRequired();
        b.Property(u => u.IsLockedOut).IsRequired();
        b.Property(u => u.LockoutEndUtc);
        b.Property(u => u.FailedLoginAttempts).IsRequired();

        // Audit fields from AggregateRoot (no explicit mapping needed — EF maps by name).

        // UserRoles is configured via the UserRoleConfiguration below.
        // EF maps _roleIds to a many-to-many table via the join configuration.

        // Domain events are not persisted.
        b.Ignore(u => u.DomainEvents);
        b.Ignore(u => u.RoleIds);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(r => r.Name).IsUnique();
        b.Property(r => r.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(r => r.Priority).IsRequired();
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permissions");
        b.HasKey(p => p.Id);
        b.Property(p => p.Code).HasMaxLength(200).IsRequired();
        b.HasIndex(p => p.Code).IsUnique();
        b.Property(p => p.ModuleName).HasMaxLength(100).IsRequired();
        b.Property(p => p.DisplayName).HasMaxLength(200).IsRequired();
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("user_roles");
        b.HasKey(ur => new { ur.UserId, ur.RoleId });
        b.HasIndex(ur => ur.RoleId);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("role_permissions");
        b.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        b.HasIndex(rp => rp.PermissionId);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.UserId).IsRequired();
        b.Property(t => t.FamilyId).IsRequired();
        b.Property(t => t.TokenHash).HasMaxLength(200).IsRequired();
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.Property(t => t.ExpiresAtUtc).IsRequired();
        b.Property(t => t.UsedAtUtc);
        b.Property(t => t.RevokedAtUtc);
        b.Property(t => t.ReplacedByTokenId);
        b.Property(t => t.CreatedByIp).HasMaxLength(64);

        b.HasIndex(t => new { t.FamilyId, t.UsedAtUtc });
        b.HasIndex(t => new { t.UserId, t.ExpiresAtUtc });

        b.Ignore(t => t.DomainEvents);
    }
}
