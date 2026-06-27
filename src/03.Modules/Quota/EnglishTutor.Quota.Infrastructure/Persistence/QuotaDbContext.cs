using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Quota.Infrastructure.Persistence;

/// <summary>
/// Database context for the Quota module.
/// </summary>
public sealed class QuotaDbContext : DbContext
{
    public const string SchemaName = "quota";

    public QuotaDbContext(DbContextOptions<QuotaDbContext> options) : base(options)
    {
    }

    public DbSet<UserQuotaRule> UserQuotaRules => Set<UserQuotaRule>();
    public DbSet<UserQuotaState> UserQuotaStates => Set<UserQuotaState>();
    public DbSet<QuotaReservation> QuotaReservations => Set<QuotaReservation>();
    public DbSet<UsageLog> UsageLogs => Set<UsageLog>();
    public DbSet<RateLimitEvent> RateLimitEvents => Set<RateLimitEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.UserQuotaRuleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserQuotaStateConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.QuotaReservationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UsageLogConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RateLimitEventConfiguration());

        // Soft-delete query filter convention from BuildingBlocks.
        modelBuilder.ApplyAggregateRootConventions();
    }
}
