using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace EnglishTutor.Practice.Infrastructure.Persistence;

public sealed class PracticeDbContext : DbContext
{
    public const string SchemaName = "practice";

    public PracticeDbContext(DbContextOptions<PracticeDbContext> options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        FixStateOfNewChildEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        FixStateOfNewChildEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void FixStateOfNewChildEntities()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is TranscriptMessage msg && msg.CreatedAtUtc == default)
                {
                    entry.State = EntityState.Added;
                }
                else if (entry.Entity is SessionEvent evt && evt.CreatedAtUtc == default)
                {
                    entry.State = EntityState.Added;
                }
            }
        }
    }

    public DbSet<PracticeSession> Sessions => Set<PracticeSession>();
    public DbSet<TranscriptMessage> TranscriptMessages => Set<TranscriptMessage>();
    public DbSet<SessionEvent> SessionEvents => Set<SessionEvent>();
    public DbSet<PracticeScenarioReadModel> ScenarioReadModels => Set<PracticeScenarioReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.PracticeSessionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TranscriptMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SessionEventConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PracticeScenarioReadModelConfiguration());

        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", SchemaName));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", SchemaName));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", SchemaName));

        // Soft-delete query filter convention
        modelBuilder.ApplyAggregateRootConventions();
    }
}
