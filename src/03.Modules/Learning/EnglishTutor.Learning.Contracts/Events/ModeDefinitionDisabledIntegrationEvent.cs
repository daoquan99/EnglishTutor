using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record ModeDefinitionDisabledIntegrationEvent(
    Guid ModeDefinitionId) : IntegrationEvent;
