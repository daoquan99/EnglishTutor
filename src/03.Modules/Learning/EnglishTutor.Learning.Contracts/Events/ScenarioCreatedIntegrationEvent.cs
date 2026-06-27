using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record ScenarioCreatedIntegrationEvent(
    Guid ScenarioId,
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string DifficultyLevel) : IntegrationEvent;
