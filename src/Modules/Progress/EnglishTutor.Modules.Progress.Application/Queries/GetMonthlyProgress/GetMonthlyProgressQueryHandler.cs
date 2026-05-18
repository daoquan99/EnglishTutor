using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetMonthlyProgress;

public sealed class GetMonthlyProgressQueryHandler(
    IProgressRepository progressRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetMonthlyProgressQuery, MonthlyProgressResponse>
{
    public async Task<Result<MonthlyProgressResponse>> Handle(GetMonthlyProgressQuery request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;
        var progress = await progressRepository.GetMonthlyProgressAsync(
            request.UserId,
            request.TargetLanguageCode,
            year,
            month,
            cancellationToken);

        return new MonthlyProgressResponse(
            request.UserId,
            request.TargetLanguageCode,
            year,
            month,
            progress?.ExpEarned ?? 0,
            progress?.ActivityCount ?? 0);
    }
}
