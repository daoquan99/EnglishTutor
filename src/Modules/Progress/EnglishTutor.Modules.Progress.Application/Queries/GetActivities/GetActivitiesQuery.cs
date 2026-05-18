using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetActivities;

public sealed record GetActivitiesQuery(Guid UserId, string TargetLanguageCode) : IQuery<IReadOnlyList<ActivityLogResponse>>;
