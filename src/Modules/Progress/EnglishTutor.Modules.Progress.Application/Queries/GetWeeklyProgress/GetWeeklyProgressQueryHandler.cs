using System.Globalization;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetWeeklyProgress;

public sealed class GetWeeklyProgressQueryHandler(
    IProgressRepository progressRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetWeeklyProgressQuery, WeeklyProgressResponse>
{
    public async Task<Result<WeeklyProgressResponse>> Handle(GetWeeklyProgressQuery request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var year = request.Year ?? ISOWeek.GetYear(now);
        var weekNumber = request.WeekNumber ?? ISOWeek.GetWeekOfYear(now);
        var progress = await progressRepository.GetWeeklyProgressAsync(
            request.UserId,
            request.TargetLanguageCode,
            year,
            weekNumber,
            cancellationToken);

        return new WeeklyProgressResponse(
            request.UserId,
            request.TargetLanguageCode,
            year,
            weekNumber,
            progress?.ExpEarned ?? 0,
            progress?.ActivityCount ?? 0);
    }
}
