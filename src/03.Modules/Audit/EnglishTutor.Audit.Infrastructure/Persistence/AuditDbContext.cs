using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext, IInboxDbContext
{
    public const string SchemaName = "audit";

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);

        modelBuilder.AddNativeInbox();
        modelBuilder.Entity<InboxMessage>().ToTable("integration_inbox_messages", SchemaName);

        modelBuilder.ApplyAggregateRootConventions();
    }
}
