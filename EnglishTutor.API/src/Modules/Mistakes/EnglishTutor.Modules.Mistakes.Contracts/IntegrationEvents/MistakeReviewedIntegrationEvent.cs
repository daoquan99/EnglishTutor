using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;

public sealed record MistakeReviewedIntegrationEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    DateTime ReviewedAtUtc) : IntegrationEvent;
