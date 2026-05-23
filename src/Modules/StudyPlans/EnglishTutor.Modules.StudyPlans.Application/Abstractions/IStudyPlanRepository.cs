using EnglishTutor.Modules.StudyPlans.Domain.Entities;

namespace EnglishTutor.Modules.StudyPlans.Application.Abstractions;

public interface IStudyPlanRepository
{
    Task<UserStudyPlan?> GetActiveByUserAndTargetLanguageAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);
    Task<UserStudyPlan?> GetByIdAsync(Guid studyPlanId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserStudyPlan>> ListActiveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserStudyPlan>> ListActiveByStudyDayAsync(DayOfWeek studyDay, CancellationToken cancellationToken);
    Task AddAsync(UserStudyPlan studyPlan, CancellationToken cancellationToken);
}
