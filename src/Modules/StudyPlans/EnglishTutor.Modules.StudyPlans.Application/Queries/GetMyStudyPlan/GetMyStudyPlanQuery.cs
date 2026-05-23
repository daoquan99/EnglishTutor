using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetMyStudyPlan;

public sealed record GetMyStudyPlanQuery(Guid UserId, string TargetLanguageCode) : IQuery<StudyPlanResponse>;
