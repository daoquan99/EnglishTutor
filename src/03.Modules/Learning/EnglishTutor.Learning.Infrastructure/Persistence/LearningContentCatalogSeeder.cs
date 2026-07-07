using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Shared;
using EnglishTutor.Learning.Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

public sealed class LearningContentCatalogSeeder(LearningDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedModesAsync(cancellationToken);
        await SeedTopicsAsync(cancellationToken);

        var topics = await dbContext.Topics
            .IgnoreQueryFilters()
            .ToDictionaryAsync(topic => topic.Slug.Value, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var modes = await dbContext.ModeDefinitions
            .IgnoreQueryFilters()
            .ToDictionaryAsync(mode => mode.Code.Value, StringComparer.OrdinalIgnoreCase, cancellationToken);

        await SeedTopicModesAsync(topics, modes, cancellationToken);
        await SeedScenariosAsync(topics, modes, cancellationToken);
        await SeedVocabularyAsync(topics, cancellationToken);
        await SeedPhrasesAsync(topics, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedModesAsync(CancellationToken cancellationToken)
    {
        var existingCodes = await dbContext.ModeDefinitions
            .IgnoreQueryFilters()
            .Select(mode => mode.Code.Value)
            .ToListAsync(cancellationToken);
        var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in LearningContentSeedCatalog.Modes.Where(seed => !existing.Contains(seed.Code)))
        {
            dbContext.ModeDefinitions.Add(ModeDefinition.Create(seed.Code, seed.Name, seed.Description, null));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTopicsAsync(CancellationToken cancellationToken)
    {
        var existingSlugs = await dbContext.Topics
            .IgnoreQueryFilters()
            .Select(topic => topic.Slug.Value)
            .ToListAsync(cancellationToken);
        var existing = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in LearningContentSeedCatalog.Topics.Where(seed => !existing.Contains(seed.Slug)))
        {
            dbContext.Topics.Add(Topic.Create(seed.Name, seed.Slug, seed.Description, null));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTopicModesAsync(
        IReadOnlyDictionary<string, Topic> topics,
        IReadOnlyDictionary<string, ModeDefinition> modes,
        CancellationToken cancellationToken)
    {
        var existing = (await dbContext.TopicModes
                .Select(item => new { item.TopicId, item.ModeDefinitionId })
                .ToListAsync(cancellationToken))
            .Select(item => (item.TopicId, item.ModeDefinitionId))
            .ToHashSet();

        foreach (var seed in LearningContentSeedCatalog.TopicModes)
        {
            if (!TryResolveActive(seed.TopicSlug, seed.ModeCode, topics, modes, out var topic, out var mode) ||
                existing.Contains((topic.Id, mode.Id)))
            {
                continue;
            }

            var topicMode = topic.EnableMode(mode.Id, seed.ConfigJson, null);
            if (topicMode is not null)
            {
                dbContext.TopicModes.Add(topicMode);
                existing.Add((topic.Id, mode.Id));
            }
        }
    }

    private async Task SeedScenariosAsync(
        IReadOnlyDictionary<string, Topic> topics,
        IReadOnlyDictionary<string, ModeDefinition> modes,
        CancellationToken cancellationToken)
    {
        var existing = (await dbContext.Scenarios
                .IgnoreQueryFilters()
                .Select(item => new { item.TopicId, item.ModeDefinitionId, item.Name })
                .ToListAsync(cancellationToken))
            .Select(item => ScenarioKey(item.TopicId, item.ModeDefinitionId, item.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in LearningContentSeedCatalog.Scenarios)
        {
            if (!TryResolveActive(seed.TopicSlug, seed.ModeCode, topics, modes, out var topic, out var mode) ||
                !existing.Add(ScenarioKey(topic.Id, mode.Id, seed.Name)))
            {
                continue;
            }

            dbContext.Scenarios.Add(Scenario.Create(
                topic.Id,
                mode.Id,
                seed.Name,
                seed.Description,
                seed.DifficultyLevel,
                seed.PromptTemplate));
        }
    }

    private async Task SeedVocabularyAsync(
        IReadOnlyDictionary<string, Topic> topics,
        CancellationToken cancellationToken)
    {
        var existing = (await dbContext.TopicVocabularies
                .IgnoreQueryFilters()
                .Select(item => new { item.TopicId, item.WordNormalized })
                .ToListAsync(cancellationToken))
            .Select(item => ContentKey(item.TopicId, item.WordNormalized))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in LearningContentSeedCatalog.Vocabulary)
        {
            if (!topics.TryGetValue(seed.TopicSlug, out var topic) || topic.IsDeleted ||
                !existing.Add(ContentKey(topic.Id, TextNormalizer.Normalize(seed.Word))))
            {
                continue;
            }

            dbContext.TopicVocabularies.Add(TopicVocabulary.Create(
                topic.Id,
                seed.Word,
                seed.Definition,
                seed.PartOfSpeech,
                seed.Phonetic,
                seed.ExampleSentence,
                seed.ExampleTranslation));
        }
    }

    private async Task SeedPhrasesAsync(
        IReadOnlyDictionary<string, Topic> topics,
        CancellationToken cancellationToken)
    {
        var existing = (await dbContext.TopicPhrases
                .IgnoreQueryFilters()
                .Select(item => new { item.TopicId, item.PhraseNormalized })
                .ToListAsync(cancellationToken))
            .Select(item => ContentKey(item.TopicId, item.PhraseNormalized))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in LearningContentSeedCatalog.Phrases)
        {
            if (!topics.TryGetValue(seed.TopicSlug, out var topic) || topic.IsDeleted ||
                !existing.Add(ContentKey(topic.Id, TextNormalizer.Normalize(seed.Phrase))))
            {
                continue;
            }

            dbContext.TopicPhrases.Add(TopicPhrase.Create(
                topic.Id,
                seed.Phrase,
                seed.Translation,
                seed.Context));
        }
    }

    private static bool TryResolveActive(
        string topicSlug,
        string modeCode,
        IReadOnlyDictionary<string, Topic> topics,
        IReadOnlyDictionary<string, ModeDefinition> modes,
        out Topic topic,
        out ModeDefinition mode)
    {
        var hasTopic = topics.TryGetValue(topicSlug, out topic!);
        var hasMode = modes.TryGetValue(modeCode, out mode!);
        return hasTopic && hasMode && !topic.IsDeleted && !mode.IsDeleted;
    }

    private static string ScenarioKey(Guid topicId, Guid modeId, string name) =>
        $"{topicId:N}|{modeId:N}|{name.Trim()}";

    private static string ContentKey(Guid topicId, string normalizedText) =>
        $"{topicId:N}|{normalizedText}";
}
