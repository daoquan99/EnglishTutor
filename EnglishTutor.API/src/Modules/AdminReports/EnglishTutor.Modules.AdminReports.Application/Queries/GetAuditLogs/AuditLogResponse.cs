namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;

public sealed record AuditLogResponse(
    Guid Id, Guid AdminUserId, string Action,
    string TargetEntity, string TargetEntityId,
    string? OldValue, string? NewValue,
    string? IpAddress, string? UserAgent, DateTime CreatedAtUtc);
