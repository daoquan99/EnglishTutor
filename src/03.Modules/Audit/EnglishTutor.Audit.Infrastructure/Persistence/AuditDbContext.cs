using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace EnglishTutor.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public const string SchemaName = "audit";

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);

        // MassTransit EF Outbox tables mapped to audit schema
        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", SchemaName));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", SchemaName));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", SchemaName));

        modelBuilder.ApplyAggregateRootConventions();
    }
}
