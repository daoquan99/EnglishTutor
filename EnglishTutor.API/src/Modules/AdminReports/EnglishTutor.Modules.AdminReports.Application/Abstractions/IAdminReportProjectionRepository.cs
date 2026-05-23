using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;

namespace EnglishTutor.Modules.AdminReports.Application.Abstractions;

public interface IAdminReportProjectionRepository
{
    Task<UserOverviewCard?> GetUserOverviewCardAsync(Guid userId, CancellationToken cancellationToken);

    Task AddUserOverviewCardAsync(UserOverviewCard card, CancellationToken cancellationToken);
}
