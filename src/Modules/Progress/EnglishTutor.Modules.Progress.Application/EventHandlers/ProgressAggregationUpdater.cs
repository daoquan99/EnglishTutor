using System.Globalization;
using EnglishTutor.Modules.Progress.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

internal static class ProgressAggregationUpdater
{
    public static async Task RecordPeriodProgressAsync(
        IProgressRepository progressRepository,
        Guid userId,
        string targetLanguageCode,
        DateTime occurredAtUtc,
        int expEarned,
        CancellationToken cancellationToken)
    {
        var daily = await progressRepository.GetOrCreateDailyProgressAsync(
            userId,
            targetLanguageCode,
            DateOnly.FromDateTime(occurredAtUtc),
            cancellationToken);
        daily.RecordActivity(expEarned);

        var weekly = await progressRepository.GetOrCreateWeeklyProgressAsync(
            userId,
            targetLanguageCode,
            ISOWeek.GetYear(occurredAtUtc),
            ISOWeek.GetWeekOfYear(occurredAtUtc),
            cancellationToken);
        weekly.RecordActivity(expEarned);

        var monthly = await progressRepository.GetOrCreateMonthlyProgressAsync(
            userId,
            targetLanguageCode,
            occurredAtUtc.Year,
            occurredAtUtc.Month,
            cancellationToken);
        monthly.RecordActivity(expEarned);
    }
}
