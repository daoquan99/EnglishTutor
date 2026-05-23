using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;

namespace EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

internal static class StudyPlanMappers
{
    public static StudyPlanResponse ToResponse(this UserStudyPlan plan) =>
        new(
            plan.Id,
            plan.UserId,
            plan.TargetLanguageCode.Value,
            plan.PreferredStudyTime,
            plan.ReminderBeforeMinutes,
            plan.TimeZoneId,
            plan.DailyTargetMinutes,
            plan.WeeklyTargetMinutes,
            plan.MonthlyTargetMinutes,
            plan.MonthlyTargetStudyDays,
            plan.IsActive,
            plan.WeekDays
                .OrderBy(weekDay => (int)weekDay.DayOfWeek)
                .Select(weekDay => new WeekScheduleResponse(weekDay.DayOfWeek, weekDay.IsStudyDay))
                .ToList());

    public static PlannedSessionResponse ToResponse(this PlannedStudySession session) =>
        new(
            session.Id,
            session.StudyPlanId,
            session.UserId,
            session.TargetLanguageCode,
            session.ScheduledDateUtc,
            session.Status.ToString(),
            session.CompletedAtUtc,
            session.MissedAtUtc);
}
