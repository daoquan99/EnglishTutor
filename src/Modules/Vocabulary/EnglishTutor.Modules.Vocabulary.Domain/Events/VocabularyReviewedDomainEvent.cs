using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Vocabulary.Domain.Events;

public sealed record VocabularyReviewedDomainEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    string MasteryStatus,
    DateTime ReviewedAtUtc) : DomainEvent;
