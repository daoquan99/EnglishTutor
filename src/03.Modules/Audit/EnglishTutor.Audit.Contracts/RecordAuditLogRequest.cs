using System;

namespace EnglishTutor.Audit.Contracts;

public sealed record RecordAuditLogRequest(
    Guid? UserId,
    string Action,
    string EntityType,
    string EntityId,
    string DetailJson,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    DateTime CreatedAtUtc);
