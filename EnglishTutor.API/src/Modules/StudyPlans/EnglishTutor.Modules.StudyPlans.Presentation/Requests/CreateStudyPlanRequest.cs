namespace EnglishTutor.Modules.StudyPlans.Presentation.Requests;

public sealed record CreateStudyPlanRequest(
    string TargetLanguageCode,
    TimeOnly PreferredStudyTime,
    int ReminderBeforeMinutes,
    string TimeZoneId,
    int DailyTargetMinutes,
    int WeeklyTargetMinutes,
    int MonthlyTargetMinutes,
    int MonthlyTargetStudyDays,
    IReadOnlyCollection<DayOfWeek>? StudyDays);
