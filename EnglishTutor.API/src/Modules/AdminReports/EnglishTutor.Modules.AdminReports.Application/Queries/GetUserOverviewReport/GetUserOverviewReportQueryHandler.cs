using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;

public sealed class GetUserOverviewReportQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetUserOverviewReportQuery, IReadOnlyList<UserOverviewCardResponse>>
{
    public async Task<Result<IReadOnlyList<UserOverviewCardResponse>>> Handle(GetUserOverviewReportQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetUserOverviewAsync(request.Page, request.PageSize, request.SortBy, cancellationToken));
}
