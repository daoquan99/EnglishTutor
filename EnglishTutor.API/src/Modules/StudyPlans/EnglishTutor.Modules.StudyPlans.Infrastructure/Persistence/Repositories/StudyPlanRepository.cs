using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Repositories;

public sealed class StudyPlanRepository(StudyPlansDbContext dbContext) : IStudyPlanRepository
{
    public Task<UserStudyPlan?> GetActiveByUserAndTargetLanguageAsync(
        Guid userId,
        string targetLanguageCode,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = targetLanguageCode.Trim().ToLowerInvariant();
        return dbContext.UserStudyPlans
            .Include(plan => plan.WeekDays)
            .Include(plan => plan.Targets)
            .SingleOrDefaultAsync(
                plan => plan.UserId == userId &&
                    EF.Property<string>(plan, nameof(UserStudyPlan.TargetLanguageCode)) == normalizedLanguage &&
                    plan.IsActive,
                cancellationToken);
    }

    public Task<UserStudyPlan?> GetByIdAsync(Guid studyPlanId, CancellationToken cancellationToken) =>
        dbContext.UserStudyPlans
            .Include(plan => plan.WeekDays)
            .Include(plan => plan.Targets)
            .SingleOrDefaultAsync(plan => plan.Id == studyPlanId, cancellationToken);

    public async Task<IReadOnlyList<UserStudyPlan>> ListActiveAsync(CancellationToken cancellationToken) =>
        await dbContext.UserStudyPlans
            .Include(plan => plan.WeekDays)
            .Where(plan => plan.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserStudyPlan>> ListActiveByStudyDayAsync(DayOfWeek studyDay, CancellationToken cancellationToken) =>
        await dbContext.UserStudyPlans
            .Include(plan => plan.WeekDays)
            .Where(plan => plan.IsActive && plan.WeekDays.Any(day => day.DayOfWeek == studyDay && day.IsStudyDay))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserStudyPlan studyPlan, CancellationToken cancellationToken) =>
        await dbContext.UserStudyPlans.AddAsync(studyPlan, cancellationToken);
}
