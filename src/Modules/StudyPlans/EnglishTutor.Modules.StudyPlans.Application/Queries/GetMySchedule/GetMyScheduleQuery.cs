using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetMySchedule;

public sealed record GetMyScheduleQuery(Guid UserId, string TargetLanguageCode) : IQuery<IReadOnlyList<WeekScheduleResponse>>;
