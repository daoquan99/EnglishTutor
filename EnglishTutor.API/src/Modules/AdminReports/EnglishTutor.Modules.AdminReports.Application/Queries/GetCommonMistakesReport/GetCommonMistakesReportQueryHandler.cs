using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;

public sealed class GetCommonMistakesReportQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetCommonMistakesReportQuery, IReadOnlyList<CommonMistakeStatResponse>>
{
    public async Task<Result<IReadOnlyList<CommonMistakeStatResponse>>> Handle(GetCommonMistakesReportQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetCommonMistakesAsync(request.TargetLanguageCode, request.Top, cancellationToken));
}
