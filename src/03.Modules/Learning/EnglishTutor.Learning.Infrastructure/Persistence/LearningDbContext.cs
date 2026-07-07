using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

public sealed class LearningDbContext : DbContext, IOutboxDbContext
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
    public DbSet<LanguageDefinition> LanguageDefinitions => Set<LanguageDefinition>();
    public DbSet<LearnerLanguagePortfolio> LearnerLanguagePortfolios => Set<LearnerLanguagePortfolio>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
        modelBuilder.ApplyConfiguration(new Configurations.LanguageDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.LearnerLanguagePortfolioConfiguration());

        modelBuilder.AddNativeOutbox();
        modelBuilder.Entity<OutboxMessage>().ToTable("integration_outbox_messages", SchemaName);
        modelBuilder.Entity<OutboxMessage>().Property(message => message.PayloadJson).HasColumnType("jsonb");

        // Soft-delete query filter convention from BuildingBlocks.
        modelBuilder.ApplyAggregateRootConventions();
    }
}
