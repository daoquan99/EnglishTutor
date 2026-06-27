using System;

namespace EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;

public sealed record SecurityEventResponseDto(
    Guid Id,
    string CategoryCode,
    string SourceModule,
    string SourceEventType,
    Guid? UserId,
    Guid? SessionId,
    Guid? RefreshTokenFamilyId,
    Guid? RefreshTokenId,
    string? ReasonCode,
    Guid? CorrelationId,
    Guid? CausationId,
    string? IpAddressHash,
    string? UserAgentHash,
    DateTime OccurredAtUtc);
