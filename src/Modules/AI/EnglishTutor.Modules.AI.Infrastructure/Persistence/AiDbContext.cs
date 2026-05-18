using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence;

public sealed class AiDbContext(DbContextOptions<AiDbContext> options) : DbContext(options)
{
    public DbSet<AiRequestLog> AiRequestLogs => Set<AiRequestLog>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<PromptVersion> PromptVersions => Set<PromptVersion>();
    public DbSet<ModelRoutingRule> ModelRoutingRules => Set<ModelRoutingRule>();
    public DbSet<AiUsageCounter> AiUsageCounters => Set<AiUsageCounter>();
    public DbSet<AiCostEstimation> AiCostEstimations => Set<AiCostEstimation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai");

        modelBuilder.Entity<AiRequestLog>(builder =>
        {
            builder.ToTable("AiRequestLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.TaskType).HasConversion<string>().HasMaxLength(100);
            builder.Property(log => log.ModelUsed).HasConversion<string>().HasMaxLength(100);
            builder.Property(log => log.Status).HasConversion<string>().HasMaxLength(50);
            builder.Property(log => log.RequestPayloadHash).HasMaxLength(128);
            builder.Property(log => log.ErrorMessage).HasMaxLength(2000);
        });

        modelBuilder.Entity<PromptTemplate>(builder =>
        {
            builder.ToTable("PromptTemplates");
            builder.HasKey(template => template.Id);
            builder.Property(template => template.Name).HasMaxLength(150);
            builder.Property(template => template.TaskType).HasConversion<string>().HasMaxLength(100);
            builder.Property(template => template.Description).HasMaxLength(500);
            builder.HasIndex(template => template.Name).IsUnique();
            builder.Metadata.FindNavigation(nameof(PromptTemplate.Versions))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PromptVersion>(builder =>
        {
            builder.ToTable("PromptVersions");
            builder.HasKey(version => version.Id);
            builder.HasIndex(version => new { version.PromptTemplateId, version.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<ModelRoutingRule>(builder =>
        {
            builder.ToTable("ModelRoutingRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.TaskType).HasConversion<string>().HasMaxLength(100);
            builder.Property(rule => rule.PreferredModel).HasConversion<string>().HasMaxLength(100);
            builder.Property(rule => rule.FallbackModel).HasConversion<string>().HasMaxLength(100);
            builder.HasIndex(rule => rule.TaskType).IsUnique();
        });

        modelBuilder.Entity<AiUsageCounter>(builder =>
        {
            builder.ToTable("AiUsageCounters");
            builder.HasKey(counter => counter.Id);
            builder.Property(counter => counter.TaskType).HasConversion<string>().HasMaxLength(100);
            builder.HasIndex(counter => new { counter.UserId, counter.TaskType, counter.UsageDate }).IsUnique();
        });

        modelBuilder.Entity<AiCostEstimation>(builder =>
        {
            builder.ToTable("AiCostEstimations");
            builder.HasKey(estimation => estimation.Id);
            builder.Property(estimation => estimation.ModelType).HasConversion<string>().HasMaxLength(100);
        });
    }
}
