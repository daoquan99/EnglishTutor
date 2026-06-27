using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence;

/// <summary>
/// Database context for the AI Gateway module.
/// </summary>
public sealed class AiGatewayDbContext : DbContext
{
    public const string SchemaName = "aigateway";

    public AiGatewayDbContext(DbContextOptions<AiGatewayDbContext> options) : base(options)
    {
    }

    public DbSet<AiProvider> Providers => Set<AiProvider>();
    public DbSet<AiModel> Models => Set<AiModel>();
    public DbSet<AiProviderKey> ProviderKeys => Set<AiProviderKey>();
    public DbSet<AiRoutingRule> RoutingRules => Set<AiRoutingRule>();
    public DbSet<AiRouteLease> RouteLeases => Set<AiRouteLease>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiGatewayDbContext).Assembly);

        // Apply canonical audit/soft-delete Conventions from BuildingBlocks
        modelBuilder.ApplyAggregateRootConventions();

        // MassTransit EF Outbox tables mapped to aigateway schema
        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", SchemaName));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", SchemaName));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", SchemaName));
    }
}
