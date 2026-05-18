using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

public sealed record VocabularyPronunciationPracticedIntegrationEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    DateTime PracticedAtUtc) : IntegrationEvent;
