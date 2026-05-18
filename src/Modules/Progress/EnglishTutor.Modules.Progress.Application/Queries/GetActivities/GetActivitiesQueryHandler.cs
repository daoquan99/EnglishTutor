using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetActivities;

public sealed class GetActivitiesQueryHandler(IProgressRepository progressRepository)
    : IQueryHandler<GetActivitiesQuery, IReadOnlyList<ActivityLogResponse>>
{
    public async Task<Result<IReadOnlyList<ActivityLogResponse>>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await progressRepository.GetRecentActivitiesAsync(request.UserId, request.TargetLanguageCode, cancellationToken);
        return activities.Select(activity => new ActivityLogResponse(
            activity.ActivityId,
            activity.ActivityType.ToString(),
            activity.CompletedAtUtc,
            activity.DurationSeconds,
            activity.ExpEarned,
            activity.Score,
            activity.Result)).ToList();
    }
}
