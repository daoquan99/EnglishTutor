using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record ModeDefinitionUpdatedIntegrationEvent(
    Guid ModeDefinitionId,
    string Code,
    string Name) : IntegrationEvent;
