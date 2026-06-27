using System;

namespace EnglishTutor.Audit.Contracts;

// Cross-module request to record any security event. Contains only IDs,
// reason code, and hashed IP/UA context. Never stores raw credential strings,
// raw tokens, or plain PII.
public sealed record RecordSecurityEventRequest(
    string CategoryCode,
    string SourceModule,
    string SourceEventType,
    Guid? UserId,
    Guid? SessionId,
    Guid? RefreshTokenFamilyId,
    Guid? RefreshTokenId,
    string? ReasonCode,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime OccurredAtUtc);
