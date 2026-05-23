using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetPlannedSessions;

public sealed class GetPlannedSessionsQueryHandler(IPlannedStudySessionRepository sessionRepository)
    : IQueryHandler<GetPlannedSessionsQuery, IReadOnlyList<PlannedSessionResponse>>
{
    public async Task<Result<IReadOnlyList<PlannedSessionResponse>>> Handle(GetPlannedSessionsQuery request, CancellationToken cancellationToken)
    {
        if (request.FromDateUtc.HasValue && request.ToDateUtc.HasValue && request.FromDateUtc > request.ToDateUtc)
        {
            return Result.Failure<IReadOnlyList<PlannedSessionResponse>>(StudyPlanErrors.InvalidDateRange);
        }

        var sessions = await sessionRepository.ListAsync(
            request.UserId,
            request.FromDateUtc,
            request.ToDateUtc,
            request.Status,
            cancellationToken);

        return sessions.Select(session => session.ToResponse()).ToList();
    }
}
