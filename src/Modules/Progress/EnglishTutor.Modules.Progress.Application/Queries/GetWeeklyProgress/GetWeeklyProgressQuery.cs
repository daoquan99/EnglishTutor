using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetWeeklyProgress;

public sealed record GetWeeklyProgressQuery(
    Guid UserId,
    string TargetLanguageCode,
    int? Year,
    int? WeekNumber) : IQuery<WeeklyProgressResponse>;
