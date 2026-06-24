using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

public sealed class LearningDbContext : DbContext
{
    public const string SchemaName = "learning";

    public LearningDbContext(DbContextOptions<LearningDbContext> options) : base(options)
    {
    }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TopicMode> TopicModes => Set<TopicMode>();
    public DbSet<ModeDefinition> ModeDefinitions => Set<ModeDefinition>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<TopicVocabulary> TopicVocabularies => Set<TopicVocabulary>();
    public DbSet<TopicPhrase> TopicPhrases => Set<TopicPhrase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new Configurations.TopicConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicModeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ModeDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ScenarioConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicVocabularyConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicPhraseConfiguration());

        // MassTransit EF Outbox tables mapped to learning schema
        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", SchemaName));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", SchemaName));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", SchemaName));

        // Soft-delete query filter convention from BuildingBlocks.
        modelBuilder.ApplyAggregateRootConventions();
    }
}
