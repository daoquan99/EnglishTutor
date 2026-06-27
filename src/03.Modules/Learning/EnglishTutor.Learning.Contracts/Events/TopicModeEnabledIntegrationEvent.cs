using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record TopicModeEnabledIntegrationEvent(
    Guid TopicId,
    Guid ModeDefinitionId,
    string ConfigJson) : IntegrationEvent;
