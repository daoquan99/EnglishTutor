using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Errors;

public static class AuditLogErrors
{
    public static Error ActionRequired() =>
        new("AuditLog.ActionRequired", "Action is required.");

    public static Error EntityTypeRequired() =>
        new("AuditLog.EntityTypeRequired", "EntityType is required.");

    public static Error EntityIdRequired() =>
        new("AuditLog.EntityIdRequired", "EntityId is required.");

    public static Error DetailJsonRequired() =>
        new("AuditLog.DetailJsonRequired", "DetailJson is required.");

    public static Error DetailJsonInvalidJson() =>
        new("AuditLog.DetailJsonInvalidJson", "DetailJson must be valid JSON.");

    public static Error DetailJsonContainsSecrets() =>
        new("AuditLog.DetailJsonContainsSecrets", "DetailJson must not contain sensitive markers.");

    public static Error CreatedAtUtcRequired() =>
        new("AuditLog.CreatedAtUtcRequired", "CreatedAtUtc must be a valid UTC timestamp.");
}
