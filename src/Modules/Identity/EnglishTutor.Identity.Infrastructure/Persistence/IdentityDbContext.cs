using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Entities;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<RefreshTokenFamily> RefreshTokenFamilies => Set<RefreshTokenFamily>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RoleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenFamilyConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserSessionConfiguration());

        // Soft-delete query filter convention from BuildingBlocks.
        modelBuilder.ApplyAggregateRootConventions();
    }
}
