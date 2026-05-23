namespace EnglishTutor.Modules.StudyPlans.Presentation.Requests;

public sealed record UpdateStudyPlanRequest(
    TimeOnly? PreferredStudyTime,
    int? ReminderBeforeMinutes,
    int? DailyTargetMinutes,
    int? WeeklyTargetMinutes,
    int? MonthlyTargetMinutes,
    int? MonthlyTargetStudyDays);
