using EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateSchedule;

namespace EnglishTutor.Modules.StudyPlans.Presentation.Requests;

public sealed record UpdateScheduleRequest(IReadOnlyCollection<DaySchedule> Days);
