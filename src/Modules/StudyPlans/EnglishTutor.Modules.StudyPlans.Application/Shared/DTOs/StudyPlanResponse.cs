namespace EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

public sealed record StudyPlanResponse(
    Guid Id,
    Guid UserId,
    string TargetLanguageCode,
    TimeOnly PreferredStudyTime,
    int ReminderBeforeMinutes,
    string TimeZoneId,
    int DailyTargetMinutes,
    int WeeklyTargetMinutes,
    int MonthlyTargetMinutes,
    int MonthlyTargetStudyDays,
    bool IsActive,
    IReadOnlyList<WeekScheduleResponse> WeekDays);
