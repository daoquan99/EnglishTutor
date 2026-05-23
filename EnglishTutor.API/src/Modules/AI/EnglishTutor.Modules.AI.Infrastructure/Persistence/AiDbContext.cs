using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
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
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiProviderModel> AiProviderModels => Set<AiProviderModel>();
    public DbSet<AiRuntimeRoute> AiRuntimeRoutes => Set<AiRuntimeRoute>();
    public DbSet<AiUsageCounter> AiUsageCounters => Set<AiUsageCounter>();
    public DbSet<AiCostEstimation> AiCostEstimations => Set<AiCostEstimation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.Ignore<DomainEvent>();

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

        modelBuilder.Entity<AiProvider>(builder =>
        {
            builder.ToTable("AiProviders");
            builder.HasKey(provider => provider.Id);
            builder.Property(provider => provider.ProviderName).HasMaxLength(100).IsRequired();
            builder.Property(provider => provider.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(provider => provider.ProviderType).HasConversion<string>().HasMaxLength(100);
            builder.Property(provider => provider.BaseUrl).HasMaxLength(500);
            builder.Property(provider => provider.ApiKeySecretName).HasMaxLength(200);
            builder.HasIndex(provider => provider.ProviderName).IsUnique();
            builder.HasMany(provider => provider.Models)
                .WithOne()
                .HasForeignKey(model => model.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Metadata.FindNavigation(nameof(AiProvider.Models))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AiProviderModel>(builder =>
        {
            builder.ToTable("AiProviderModels");
            builder.HasKey(model => model.Id);
            builder.Property(model => model.ModelCode).HasMaxLength(150).IsRequired();
            builder.Property(model => model.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(model => model.Capability).HasConversion<string>().HasMaxLength(100);
            builder.HasIndex(model => new { model.ProviderId, model.ModelCode, model.Capability }).IsUnique();
            builder.HasIndex(model => new { model.Capability, model.IsEnabled, model.Priority });
        });

        modelBuilder.Entity<AiRuntimeRoute>(builder =>
        {
            builder.ToTable("AiRuntimeRoutes");
            builder.HasKey(route => route.Id);
            builder.Property(route => route.TaskType).HasConversion<string>().HasMaxLength(100);
            builder.Property(route => route.Capability).HasConversion<string>().HasMaxLength(100);
            builder.Property(route => route.PreferredProviderName).HasMaxLength(100).IsRequired();
            builder.Property(route => route.PreferredModelCode).HasMaxLength(150).IsRequired();
            builder.Property(route => route.FallbackProviderName).HasMaxLength(100);
            builder.Property(route => route.FallbackModelCode).HasMaxLength(150);
            builder.HasIndex(route => new { route.TaskType, route.Capability }).IsUnique();
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

        modelBuilder.ApplySoftDeleteQueryFilters();
    }
}
