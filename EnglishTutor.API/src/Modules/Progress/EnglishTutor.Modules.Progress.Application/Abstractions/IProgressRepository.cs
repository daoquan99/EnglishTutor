using EnglishTutor.Modules.Progress.Domain.Entities;

namespace EnglishTutor.Modules.Progress.Application.Abstractions;

public interface IProgressRepository
{
    Task<UserExperience> GetOrCreateExperienceAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<UserStreak> GetOrCreateStreakAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<UserSkillProgress> GetOrCreateSkillProgressAsync(Guid userId, string targetLanguageCode, BuildingBlocks.SharedKernel.LearningSkill skill, CancellationToken cancellationToken);

    Task<UserDashboardSnapshot> GetOrCreateDashboardSnapshotAsync(Guid userId, string targetLanguageCode, DateOnly date, CancellationToken cancellationToken);

    Task<UserDailyProgress> GetOrCreateDailyProgressAsync(Guid userId, string targetLanguageCode, DateOnly date, CancellationToken cancellationToken);

    Task<UserWeeklyProgress> GetOrCreateWeeklyProgressAsync(Guid userId, string targetLanguageCode, int year, int weekNumber, CancellationToken cancellationToken);

    Task<UserMonthlyProgress> GetOrCreateMonthlyProgressAsync(Guid userId, string targetLanguageCode, int year, int month, CancellationToken cancellationToken);

    Task AddActivityLogAsync(LearningActivityLog activityLog, CancellationToken cancellationToken);

    Task<IReadOnlyList<LearningActivityLog>> GetRecentActivitiesAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<UserWeeklyProgress?> GetWeeklyProgressAsync(Guid userId, string targetLanguageCode, int year, int weekNumber, CancellationToken cancellationToken);

    Task<UserMonthlyProgress?> GetMonthlyProgressAsync(Guid userId, string targetLanguageCode, int year, int month, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSkillProgress>> GetSkillProgressAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);
}
