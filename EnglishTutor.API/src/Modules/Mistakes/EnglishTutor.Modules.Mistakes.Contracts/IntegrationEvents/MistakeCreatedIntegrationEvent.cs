using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;

public sealed record MistakeCreatedIntegrationEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    string Type,
    string SourceType,
    Guid SourceId,
    DateTime CreatedAtUtc) : IntegrationEvent;
