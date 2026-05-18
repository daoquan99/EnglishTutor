using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetMonthlyProgress;

public sealed record GetMonthlyProgressQuery(
    Guid UserId,
    string TargetLanguageCode,
    int? Year,
    int? Month) : IQuery<MonthlyProgressResponse>;
