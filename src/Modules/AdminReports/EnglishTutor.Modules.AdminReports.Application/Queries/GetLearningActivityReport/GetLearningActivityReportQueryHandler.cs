using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;

public sealed class GetLearningActivityReportQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetLearningActivityReportQuery, IReadOnlyList<LearningActivityReportResponse>>
{
    public async Task<Result<IReadOnlyList<LearningActivityReportResponse>>> Handle(GetLearningActivityReportQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetLearningActivityAsync(request.From, request.To, request.Period, cancellationToken));
}
