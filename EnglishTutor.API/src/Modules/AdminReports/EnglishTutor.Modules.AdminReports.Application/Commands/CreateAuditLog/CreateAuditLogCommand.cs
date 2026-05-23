using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.CreateAuditLog;

public sealed record CreateAuditLogCommand(
    Guid AdminUserId,
    string Action,
    string TargetEntity,
    string TargetEntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent) : ICommand;
