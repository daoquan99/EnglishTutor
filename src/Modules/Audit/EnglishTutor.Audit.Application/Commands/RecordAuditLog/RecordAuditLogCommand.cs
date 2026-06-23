using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Audit.Application.Commands.RecordAuditLog;

public sealed record RecordAuditLogCommand(
    Guid? UserId,
    string Action,
    string EntityType,
    string EntityId,
    string DetailJson,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    DateTime CreatedAtUtc) : ICommand;
