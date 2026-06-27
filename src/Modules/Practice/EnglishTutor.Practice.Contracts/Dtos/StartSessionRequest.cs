using System;

namespace EnglishTutor.Practice.Contracts.Dtos;

public sealed record StartSessionRequest(
    Guid ScenarioId,
    string IdempotencyKey,
    int RequestedMinutes = 15);
