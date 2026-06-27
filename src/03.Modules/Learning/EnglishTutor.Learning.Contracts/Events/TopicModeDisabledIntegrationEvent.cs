using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record TopicModeDisabledIntegrationEvent(
    Guid TopicId,
    Guid ModeDefinitionId) : IntegrationEvent;
