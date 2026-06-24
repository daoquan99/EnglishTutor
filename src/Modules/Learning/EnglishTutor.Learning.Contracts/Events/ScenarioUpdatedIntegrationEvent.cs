using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record ScenarioUpdatedIntegrationEvent(
    Guid ScenarioId,
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string DifficultyLevel,
    string PromptTemplate) : IntegrationEvent;
