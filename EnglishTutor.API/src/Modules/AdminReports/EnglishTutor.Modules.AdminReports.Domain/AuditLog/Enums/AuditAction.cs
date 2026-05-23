namespace EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;

public enum AuditAction
{
    UserLevelOverride,
    ContentPublished,
    ContentModified,
    PromptTemplateUpdated,
    ModelRoutingChanged,
    AssessmentResultOverride,
    UserDeactivated,
    DeadLetterReprocessed,
    AdminCommandExecuted
}
