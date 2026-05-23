using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetActivities;

public sealed record GetActivitiesQuery(Guid UserId, string TargetLanguageCode) : IQuery<IReadOnlyList<ActivityLogResponse>>;
