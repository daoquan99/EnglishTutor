using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence;

public sealed class ProgressRepository(
    ProgressDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IProgressRepository
{
    public async Task<UserExperience> GetOrCreateExperienceAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var experience = await dbContext.UserExperiences
            .Include(item => item.Transactions)
            .SingleOrDefaultAsync(item => item.UserId == userId && item.TargetLanguageCode == languageCode, cancellationToken);

        if (experience is not null)
        {
            return experience;
        }

        experience = UserExperience.Create(userId, languageCode, dateTimeProvider.UtcNow);
        dbContext.UserExperiences.Add(experience);
        return experience;
    }

    public async Task<UserStreak> GetOrCreateStreakAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var streak = await dbContext.UserStreaks.SingleOrDefaultAsync(item => item.UserId == userId && item.TargetLanguageCode == languageCode, cancellationToken);
        if (streak is not null)
        {
            return streak;
        }

        streak = UserStreak.Create(userId, languageCode);
        dbContext.UserStreaks.Add(streak);
        return streak;
    }

    public async Task<UserSkillProgress> GetOrCreateSkillProgressAsync(Guid userId, string targetLanguageCode, LearningSkill skill, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var progress = await dbContext.UserSkillProgresses.SingleOrDefaultAsync(
            item => item.UserId == userId && item.TargetLanguageCode == languageCode && item.Skill == skill,
            cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        progress = UserSkillProgress.Create(userId, languageCode, skill, dateTimeProvider.UtcNow);
        dbContext.UserSkillProgresses.Add(progress);
        return progress;
    }

    public async Task<UserDashboardSnapshot> GetOrCreateDashboardSnapshotAsync(Guid userId, string targetLanguageCode, DateOnly date, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var snapshot = await dbContext.UserDashboardSnapshots.SingleOrDefaultAsync(
            item => item.UserId == userId && item.TargetLanguageCode == languageCode && item.Date == date,
            cancellationToken);
        if (snapshot is not null)
        {
            return snapshot;
        }

        var previousLevel = await dbContext.UserDashboardSnapshots
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.TargetLanguageCode == languageCode)
            .OrderByDescending(item => item.Date)
            .Select(item => item.CurrentLevel)
            .FirstOrDefaultAsync(cancellationToken);

        snapshot = UserDashboardSnapshot.Create(userId, languageCode, date, dateTimeProvider.UtcNow, previousLevel);
        dbContext.UserDashboardSnapshots.Add(snapshot);
        return snapshot;
    }

    public async Task<UserDailyProgress> GetOrCreateDailyProgressAsync(Guid userId, string targetLanguageCode, DateOnly date, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var progress = await dbContext.UserDailyProgresses.SingleOrDefaultAsync(
            item => item.UserId == userId && item.TargetLanguageCode == languageCode && item.Date == date,
            cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        progress = UserDailyProgress.Create(userId, languageCode, date);
        dbContext.UserDailyProgresses.Add(progress);
        return progress;
    }

    public async Task<UserWeeklyProgress> GetOrCreateWeeklyProgressAsync(Guid userId, string targetLanguageCode, int year, int weekNumber, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var progress = await dbContext.UserWeeklyProgresses.SingleOrDefaultAsync(
            item => item.UserId == userId && item.TargetLanguageCode == languageCode && item.Year == year && item.WeekNumber == weekNumber,
            cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        progress = UserWeeklyProgress.Create(userId, languageCode, year, weekNumber);
        dbContext.UserWeeklyProgresses.Add(progress);
        return progress;
    }

    public async Task<UserMonthlyProgress> GetOrCreateMonthlyProgressAsync(Guid userId, string targetLanguageCode, int year, int month, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        var progress = await dbContext.UserMonthlyProgresses.SingleOrDefaultAsync(
            item => item.UserId == userId && item.TargetLanguageCode == languageCode && item.Year == year && item.Month == month,
            cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        progress = UserMonthlyProgress.Create(userId, languageCode, year, month);
        dbContext.UserMonthlyProgresses.Add(progress);
        return progress;
    }

    public Task AddActivityLogAsync(LearningActivityLog activityLog, CancellationToken cancellationToken)
    {
        dbContext.LearningActivityLogs.Add(activityLog);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<LearningActivityLog>> GetRecentActivitiesAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return await dbContext.LearningActivityLogs
            .Where(log => log.UserId == userId && log.TargetLanguageCode == languageCode)
            .OrderByDescending(log => log.CompletedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public Task<UserWeeklyProgress?> GetWeeklyProgressAsync(Guid userId, string targetLanguageCode, int year, int weekNumber, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return dbContext.UserWeeklyProgresses.SingleOrDefaultAsync(
            progress => progress.UserId == userId &&
                progress.TargetLanguageCode == languageCode &&
                progress.Year == year &&
                progress.WeekNumber == weekNumber,
            cancellationToken);
    }

    public Task<UserMonthlyProgress?> GetMonthlyProgressAsync(Guid userId, string targetLanguageCode, int year, int month, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return dbContext.UserMonthlyProgresses.SingleOrDefaultAsync(
            progress => progress.UserId == userId &&
                progress.TargetLanguageCode == languageCode &&
                progress.Year == year &&
                progress.Month == month,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UserSkillProgress>> GetSkillProgressAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return await dbContext.UserSkillProgresses
            .Where(progress => progress.UserId == userId && progress.TargetLanguageCode == languageCode)
            .OrderBy(progress => progress.Skill)
            .ToListAsync(cancellationToken);
    }
}
