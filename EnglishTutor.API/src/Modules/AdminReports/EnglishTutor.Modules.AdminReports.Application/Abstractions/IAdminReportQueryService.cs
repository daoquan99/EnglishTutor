using EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;

namespace EnglishTutor.Modules.AdminReports.Application.Abstractions;

public interface IAdminReportQueryService
{
    Task<IReadOnlyList<UserOverviewCardResponse>> GetUserOverviewAsync(int page, int pageSize, string? sortBy, CancellationToken cancellationToken);
    Task<IReadOnlyList<DailyAiUsageReportResponse>> GetAiUsageAsync(DateOnly? from, DateOnly? to, string? modelType, CancellationToken cancellationToken);
    Task<IReadOnlyList<LearningActivityReportResponse>> GetLearningActivityAsync(DateOnly? from, DateOnly? to, string? period, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommonMistakeStatResponse>> GetCommonMistakesAsync(string? targetLanguageCode, int top, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssessmentPassRateReportResponse>> GetAssessmentPassRatesAsync(DateOnly? from, DateOnly? to, string? targetLanguageCode, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, string? action, string? targetEntity, CancellationToken cancellationToken);
    Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(int page, int pageSize, string? sourceModule, string? eventType, string? status, CancellationToken cancellationToken);
    Task<bool> ReprocessDeadLetterAsync(Guid deadLetterId, CancellationToken cancellationToken);
}
