using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Vocabulary.Domain.Events;

public sealed record ExampleFillBlankAttemptedDomainEvent(
    Guid UserId,
    Guid VocabularyItemId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    DateTime AttemptedAtUtc) : DomainEvent;
