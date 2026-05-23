using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateSchedule;

public sealed record UpdateScheduleCommand(
    Guid UserId,
    string TargetLanguageCode,
    IReadOnlyCollection<DaySchedule> Days) : ICommand<IReadOnlyList<WeekScheduleResponse>>;
