using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Infrastructure;
using EnglishTutor.Learning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Learning;

public sealed class LearningContentCatalogSeederTests
{
    [Fact]
    public async Task SeedAsync_EmptyDatabaseAndRerun_ProducesExactCatalogWithoutDuplicates()
    {
        await using var dbContext = CreateDbContext();
        var seeder = new LearningContentCatalogSeeder(dbContext);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        (await dbContext.Topics.IgnoreQueryFilters().CountAsync()).Should().Be(10);
        (await dbContext.ModeDefinitions.IgnoreQueryFilters().CountAsync()).Should().Be(7);
        (await dbContext.TopicModes.CountAsync()).Should().Be(40);
        (await dbContext.Scenarios.IgnoreQueryFilters().CountAsync()).Should().Be(20);
        (await dbContext.TopicVocabularies.IgnoreQueryFilters().CountAsync()).Should().Be(100);
        (await dbContext.TopicPhrases.IgnoreQueryFilters().CountAsync()).Should().Be(50);
    }

    [Fact]
    public async Task SeedAsync_PartialAndSoftDeletedCatalog_AddsOnlyMissingNaturalKeys()
    {
        await using var dbContext = CreateDbContext();
        var existing = Topic.Create("Daily Life", "daily-life", "Administrator-owned description", null);
        existing.MarkDeleted(null, DateTime.UtcNow);
        dbContext.Topics.Add(existing);
        await dbContext.SaveChangesAsync();

        var seeder = new LearningContentCatalogSeeder(dbContext);
        await seeder.SeedAsync();

        var dailyLifeRows = await dbContext.Topics
            .IgnoreQueryFilters()
            .Where(x => x.Slug.Value == "daily-life")
            .ToListAsync();

        dailyLifeRows.Should().ContainSingle();
        dailyLifeRows[0].IsDeleted.Should().BeTrue();
        dailyLifeRows[0].Description.Should().Be("Administrator-owned description");
        (await dbContext.Topics.IgnoreQueryFilters().CountAsync()).Should().Be(10);
    }

    [Fact]
    public void AddLearningInfrastructure_RegistersContentSeederAsScoped()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();

        services.AddLearningInfrastructure(configuration);

        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(LearningContentCatalogSeeder) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    private static LearningDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LearningDbContext>()
            .UseInMemoryDatabase($"learning-content-seed-{Guid.NewGuid():N}")
            .Options;

        return new LearningDbContext(options);
    }
}
