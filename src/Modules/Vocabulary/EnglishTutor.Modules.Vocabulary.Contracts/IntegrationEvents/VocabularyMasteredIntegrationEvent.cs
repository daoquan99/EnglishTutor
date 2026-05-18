using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

public sealed record VocabularyMasteredIntegrationEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    DateTime MasteredAtUtc) : IntegrationEvent;
