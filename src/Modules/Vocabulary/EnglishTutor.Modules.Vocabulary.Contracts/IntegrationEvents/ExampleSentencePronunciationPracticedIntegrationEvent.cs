using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

public sealed record ExampleSentencePronunciationPracticedIntegrationEvent(
    Guid UserId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    DateTime PracticedAtUtc) : IntegrationEvent;
