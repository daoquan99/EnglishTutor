using System;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Practice.Contracts.Events;

public sealed record PracticeSessionEndedIntegrationEventV1(
    Guid SessionId,
    Guid UserId,
    int DurationSeconds,
    string EndReason) : IntegrationEvent;
