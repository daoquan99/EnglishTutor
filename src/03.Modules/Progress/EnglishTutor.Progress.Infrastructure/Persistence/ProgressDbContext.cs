using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Progress.Infrastructure.Persistence;

public sealed class ProgressDbContext : DbContext
{
    public const string SchemaName = "progress";

    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options)
    {
    }

    public DbSet<LearnerLanguageProgress> LearnerLanguageProgress => Set<LearnerLanguageProgress>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new Configurations.LearnerLanguageProgressConfiguration());
        modelBuilder.AddNativeInbox();
        modelBuilder.ApplyAggregateRootConventions();
    }
}
