using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.CreateStudyPlan;

public sealed record CreateStudyPlanCommand(
    Guid UserId,
    string TargetLanguageCode,
    TimeOnly PreferredStudyTime,
    int ReminderBeforeMinutes,
    string TimeZoneId,
    int DailyTargetMinutes,
    int WeeklyTargetMinutes,
    int MonthlyTargetMinutes,
    int MonthlyTargetStudyDays,
    IReadOnlyCollection<DayOfWeek>? StudyDays) : ICommand<StudyPlanResponse>;
