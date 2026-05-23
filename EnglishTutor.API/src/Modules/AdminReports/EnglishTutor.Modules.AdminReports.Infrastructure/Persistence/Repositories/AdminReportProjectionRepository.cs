using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AdminReports.Infrastructure.Persistence.Repositories;

public sealed class AdminReportProjectionRepository(AdminReportsDbContext dbContext) : IAdminReportProjectionRepository
{
    public Task<UserOverviewCard?> GetUserOverviewCardAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserOverviewCards.SingleOrDefaultAsync(card => card.UserId == userId, cancellationToken);

    public async Task AddUserOverviewCardAsync(UserOverviewCard card, CancellationToken cancellationToken) =>
        await dbContext.UserOverviewCards.AddAsync(card, cancellationToken);
}
