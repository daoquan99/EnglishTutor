using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Feedback.Infrastructure.Persistence;

public sealed class FeedbackDbContext : DbContext, IOutboxDbContext, IInboxDbContext
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
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.SessionFeedbackConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CorrectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ExtractedVocabularyConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MistakePatternConfiguration());

        modelBuilder.AddNativeOutbox();
        modelBuilder.Entity<OutboxMessage>().ToTable("integration_outbox_messages", SchemaName);
        modelBuilder.Entity<OutboxMessage>().Property(message => message.PayloadJson).HasColumnType("jsonb");
        modelBuilder.AddNativeInbox();
        modelBuilder.Entity<InboxMessage>().ToTable("integration_inbox_messages", SchemaName);

        modelBuilder.ApplyAggregateRootConventions();
    }
}
