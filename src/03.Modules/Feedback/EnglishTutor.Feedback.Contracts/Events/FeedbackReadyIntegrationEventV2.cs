using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Feedback.Contracts.Events;

public sealed record FeedbackReadyIntegrationEventV2(
    Guid FeedbackId,
    Guid SessionId,
    Guid UserId,
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    int? Score,
    string? CefrLevel) : IntegrationEvent;
