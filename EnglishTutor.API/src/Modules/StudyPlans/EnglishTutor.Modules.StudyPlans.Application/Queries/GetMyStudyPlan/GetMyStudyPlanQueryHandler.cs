using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetMyStudyPlan;

public sealed class GetMyStudyPlanQueryHandler(IStudyPlanRepository studyPlanRepository)
    : IQueryHandler<GetMyStudyPlanQuery, StudyPlanResponse>
{
    public async Task<Result<StudyPlanResponse>> Handle(GetMyStudyPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await studyPlanRepository.GetActiveByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);

        return plan is null
            ? Result.Failure<StudyPlanResponse>(StudyPlanErrors.PlanNotFound)
            : plan.ToResponse();
    }
}
