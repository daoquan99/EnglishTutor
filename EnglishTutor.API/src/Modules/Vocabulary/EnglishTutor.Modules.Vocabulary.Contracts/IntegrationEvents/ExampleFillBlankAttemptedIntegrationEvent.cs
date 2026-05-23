using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

public sealed record ExampleFillBlankAttemptedIntegrationEvent(
    Guid UserId,
    Guid VocabularyItemId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score,
    DateTime AttemptedAtUtc) : IntegrationEvent;
