using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetMySchedule;

public sealed class GetMyScheduleQueryHandler(IStudyPlanRepository studyPlanRepository)
    : IQueryHandler<GetMyScheduleQuery, IReadOnlyList<WeekScheduleResponse>>
{
    public async Task<Result<IReadOnlyList<WeekScheduleResponse>>> Handle(GetMyScheduleQuery request, CancellationToken cancellationToken)
    {
        var plan = await studyPlanRepository.GetActiveByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);

        return plan is null
            ? Result.Failure<IReadOnlyList<WeekScheduleResponse>>(StudyPlanErrors.PlanNotFound)
            : Result.Success<IReadOnlyList<WeekScheduleResponse>>(plan.ToResponse().WeekDays);
    }
}
