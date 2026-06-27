using System;

namespace EnglishTutor.Audit.Application.Queries.SearchAuditLogs;

public sealed record AuditLogResponseDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string EntityType,
    string EntityId,
    string DetailJson,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    DateTime CreatedAtUtc);
