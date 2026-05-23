using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

public sealed record VocabularyReviewedIntegrationEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    string MasteryStatus,
    DateTime ReviewedAtUtc) : IntegrationEvent;
