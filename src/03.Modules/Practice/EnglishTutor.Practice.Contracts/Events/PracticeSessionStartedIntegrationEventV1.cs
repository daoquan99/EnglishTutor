using System;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Practice.Contracts.Events;

public sealed record PracticeSessionStartedIntegrationEventV1(
    Guid SessionId,
    Guid UserId,
    Guid ScenarioId,
    Guid ModeDefinitionId) : IntegrationEvent;
