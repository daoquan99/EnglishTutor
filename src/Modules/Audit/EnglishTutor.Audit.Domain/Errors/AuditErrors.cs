using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Audit.Domain.Errors;

public static class AuditErrors
{
    public static Error SecurityEventIdRequired() =>
        new("Audit.SecurityEventIdRequired", "Security event Id is required.");

    public static Error SecurityEventCategoryRequired() =>
        new("Audit.SecurityEventCategoryRequired", "CategoryCode is required.");
}
