using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;

namespace EnglishTutor.Modules.AdminReports.Domain.AuditLog;

public sealed class AuditLog : Entity<Guid>
{
    public Guid AdminUserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string TargetEntity { get; private set; } = string.Empty;
    public string TargetEntityId { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid adminUserId,
        AuditAction action,
        string targetEntity,
        string targetEntityId,
        string? oldValue,
        string? newValue,
        string? ipAddress,
        string? userAgent) =>
        new()
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetEntity = targetEntity.Trim(),
            TargetEntityId = targetEntityId.Trim(),
            OldValue = string.IsNullOrWhiteSpace(oldValue) ? null : oldValue.Trim(),
            NewValue = string.IsNullOrWhiteSpace(newValue) ? null : newValue.Trim(),
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim()
        };
}
