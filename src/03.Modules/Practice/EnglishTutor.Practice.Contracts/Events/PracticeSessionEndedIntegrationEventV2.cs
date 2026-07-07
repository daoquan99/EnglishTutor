using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Practice.Contracts.Events;

public sealed record PracticeSessionEndedIntegrationEventV2(
    Guid SessionId,
    Guid UserId,
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    int DurationSeconds,
    string EndReason) : IntegrationEvent;
