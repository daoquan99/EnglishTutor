using EnglishTutor.Learning.Infrastructure.Persistence.SeedData;
using FluentAssertions;

namespace EnglishTutor.IntegrationTests.Learning;

public sealed class LearningContentSeedCatalogTests
{
    [Fact]
    public void Catalog_HasExpectedCountsAndValidReferences()
    {
        LearningContentSeedCatalog.Topics.Should().HaveCount(10);
        LearningContentSeedCatalog.Modes.Should().HaveCount(7);
        LearningContentSeedCatalog.Scenarios.Should().HaveCount(20);
        LearningContentSeedCatalog.Vocabulary.Should().HaveCount(100);
        LearningContentSeedCatalog.Phrases.Should().HaveCount(50);

        var topicSlugs = LearningContentSeedCatalog.Topics.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var modeCodes = LearningContentSeedCatalog.Modes.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        topicSlugs.Should().HaveCount(10);
        modeCodes.Should().HaveCount(7);
        LearningContentSeedCatalog.Scenarios.Should().OnlyContain(x => topicSlugs.Contains(x.TopicSlug));
        LearningContentSeedCatalog.Scenarios.Should().OnlyContain(x => modeCodes.Contains(x.ModeCode));
        LearningContentSeedCatalog.TopicModes.Should().OnlyContain(x => topicSlugs.Contains(x.TopicSlug));
        LearningContentSeedCatalog.TopicModes.Should().OnlyContain(x => modeCodes.Contains(x.ModeCode));
        LearningContentSeedCatalog.Vocabulary.Should().OnlyContain(x => topicSlugs.Contains(x.TopicSlug));
        LearningContentSeedCatalog.Phrases.Should().OnlyContain(x => topicSlugs.Contains(x.TopicSlug));
    }

    [Fact]
    public void Catalog_NaturalKeysAreUniqueAndContentIsComplete()
    {
        LearningContentSeedCatalog.Scenarios
            .Select(x => $"{x.TopicSlug}|{x.ModeCode}|{x.Name}".ToLowerInvariant())
            .Should().OnlyHaveUniqueItems();

        LearningContentSeedCatalog.Vocabulary
            .Select(x => $"{x.TopicSlug}|{x.Word}".ToLowerInvariant())
            .Should().OnlyHaveUniqueItems();

        LearningContentSeedCatalog.Phrases
            .Select(x => $"{x.TopicSlug}|{x.Phrase}".ToLowerInvariant())
            .Should().OnlyHaveUniqueItems();

        LearningContentSeedCatalog.TopicModes
            .Select(x => $"{x.TopicSlug}|{x.ModeCode}".ToLowerInvariant())
            .Should().OnlyHaveUniqueItems();

        LearningContentSeedCatalog.Scenarios.Should().OnlyContain(x =>
            new[] { "Beginner", "Intermediate", "Advanced" }.Contains(x.DifficultyLevel) &&
            !string.IsNullOrWhiteSpace(x.PromptTemplate));

        LearningContentSeedCatalog.Vocabulary.Should().OnlyContain(x =>
            !string.IsNullOrWhiteSpace(x.Definition) &&
            !string.IsNullOrWhiteSpace(x.ExampleSentence) &&
            !string.IsNullOrWhiteSpace(x.ExampleTranslation));

        LearningContentSeedCatalog.Phrases.Should().OnlyContain(x =>
            !string.IsNullOrWhiteSpace(x.Translation) &&
            !string.IsNullOrWhiteSpace(x.Context));
    }
}
