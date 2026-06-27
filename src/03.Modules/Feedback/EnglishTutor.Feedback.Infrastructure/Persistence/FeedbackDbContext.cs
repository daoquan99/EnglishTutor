using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace EnglishTutor.Feedback.Infrastructure.Persistence;

public sealed class FeedbackDbContext : DbContext
{
    public const string SchemaName = "feedback";

    public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options)
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
                if (entry.Entity is Correction corr && corr.CreatedAtUtc == default)
                {
                    entry.State = EntityState.Added;
                }
                else if (entry.Entity is ExtractedVocabulary vocab && vocab.CreatedAtUtc == default)
                {
                    entry.State = EntityState.Added;
                }
                else if (entry.Entity is MistakePattern pat && pat.CreatedAtUtc == default)
                {
                    entry.State = EntityState.Added;
                }
            }
        }
    }

    public DbSet<SessionFeedback> SessionFeedbacks => Set<SessionFeedback>();
    public DbSet<Correction> Corrections => Set<Correction>();
    public DbSet<ExtractedVocabulary> ExtractedVocabularies => Set<ExtractedVocabulary>();
    public DbSet<MistakePattern> MistakePatterns => Set<MistakePattern>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.SessionFeedbackConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CorrectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ExtractedVocabularyConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MistakePatternConfiguration());

        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", SchemaName));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", SchemaName));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", SchemaName));

        modelBuilder.ApplyAggregateRootConventions();
    }
}
