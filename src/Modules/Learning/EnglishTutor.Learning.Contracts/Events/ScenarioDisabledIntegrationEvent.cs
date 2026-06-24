using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record ScenarioDisabledIntegrationEvent(
    Guid ScenarioId) : IntegrationEvent;
