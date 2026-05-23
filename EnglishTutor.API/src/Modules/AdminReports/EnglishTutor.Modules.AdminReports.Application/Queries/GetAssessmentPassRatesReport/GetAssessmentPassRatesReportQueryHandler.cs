using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;

public sealed class GetAssessmentPassRatesReportQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetAssessmentPassRatesReportQuery, IReadOnlyList<AssessmentPassRateReportResponse>>
{
    public async Task<Result<IReadOnlyList<AssessmentPassRateReportResponse>>> Handle(GetAssessmentPassRatesReportQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetAssessmentPassRatesAsync(request.From, request.To, request.TargetLanguageCode, cancellationToken));
}
