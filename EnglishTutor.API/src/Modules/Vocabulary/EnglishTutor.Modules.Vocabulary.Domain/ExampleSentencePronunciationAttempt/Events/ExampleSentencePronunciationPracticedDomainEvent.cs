using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Vocabulary.Domain.Events;

public sealed record ExampleSentencePronunciationPracticedDomainEvent(
    Guid UserId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    DateTime PracticedAtUtc) : DomainEvent;
