using EnglishTutor.Modules.StudyPlans.Domain.Entities;

namespace EnglishTutor.Modules.StudyPlans.Application.Shared.Services;

internal static class StudyPlanSessionGenerator
{
    public static IReadOnlyList<PlannedStudySession> GenerateNextSevenDays(UserStudyPlan plan, DateTime utcNow)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(plan.TimeZoneId);
        var studyDays = plan.WeekDays
            .Where(weekDay => weekDay.IsStudyDay)
            .Select(weekDay => weekDay.DayOfWeek)
            .ToHashSet();

        var sessions = new List<PlannedStudySession>();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), timeZone);
        var startDate = DateOnly.FromDateTime(localNow);

        for (var offset = 0; offset < 7; offset++)
        {
            var localDate = startDate.AddDays(offset);
            if (!studyDays.Contains(localDate.DayOfWeek))
            {
                continue;
            }

            var localScheduledAt = localDate.ToDateTime(plan.PreferredStudyTime, DateTimeKind.Unspecified);
            if (timeZone.IsInvalidTime(localScheduledAt))
            {
                continue;
            }

            var scheduledAtUtc = TimeZoneInfo.ConvertTimeToUtc(localScheduledAt, timeZone);
            sessions.Add(PlannedStudySession.Create(
                plan.Id,
                plan.UserId,
                plan.TargetLanguageCode.Value,
                scheduledAtUtc,
                utcNow));
        }

        return sessions;
    }
}
