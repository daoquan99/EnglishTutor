using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;

public sealed class GetAiUsageReportQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetAiUsageReportQuery, IReadOnlyList<DailyAiUsageReportResponse>>
{
    public async Task<Result<IReadOnlyList<DailyAiUsageReportResponse>>> Handle(GetAiUsageReportQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetAiUsageAsync(request.From, request.To, request.ModelType, cancellationToken));
}
