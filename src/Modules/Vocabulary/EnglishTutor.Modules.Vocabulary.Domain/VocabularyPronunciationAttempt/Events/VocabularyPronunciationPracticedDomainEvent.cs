using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Vocabulary.Domain.Events;

public sealed record VocabularyPronunciationPracticedDomainEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    DateTime PracticedAtUtc) : DomainEvent;
