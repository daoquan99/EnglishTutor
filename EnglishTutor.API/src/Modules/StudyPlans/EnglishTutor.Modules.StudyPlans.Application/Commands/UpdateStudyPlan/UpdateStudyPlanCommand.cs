using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateStudyPlan;

public sealed record UpdateStudyPlanCommand(
    Guid UserId,
    string TargetLanguageCode,
    TimeOnly? PreferredStudyTime,
    int? ReminderBeforeMinutes,
    int? DailyTargetMinutes,
    int? WeeklyTargetMinutes,
    int? MonthlyTargetMinutes,
    int? MonthlyTargetStudyDays) : ICommand<StudyPlanResponse>;
