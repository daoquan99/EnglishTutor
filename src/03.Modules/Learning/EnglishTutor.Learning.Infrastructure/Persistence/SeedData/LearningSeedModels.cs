namespace EnglishTutor.Learning.Infrastructure.Persistence.SeedData;

public sealed record TopicSeed(string Slug, string Name, string Description);

public sealed record ModeSeed(string Code, string Name, string Description);

public sealed record TopicModeSeed(string TopicSlug, string ModeCode, string? ConfigJson = null);

public sealed record ScenarioSeed(
    string TopicSlug,
    string ModeCode,
    string Name,
    string Description,
    string DifficultyLevel,
    string PromptTemplate);

public sealed record VocabularySeed(
    string TopicSlug,
    string Word,
    string Definition,
    string PartOfSpeech,
    string Phonetic,
    string ExampleSentence,
    string ExampleTranslation);

public sealed record PhraseSeed(
    string TopicSlug,
    string Phrase,
    string Translation,
    string Context);
