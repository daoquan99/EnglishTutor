using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;

public sealed record MistakeMasteredIntegrationEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    DateTime MasteredAtUtc) : IntegrationEvent;
