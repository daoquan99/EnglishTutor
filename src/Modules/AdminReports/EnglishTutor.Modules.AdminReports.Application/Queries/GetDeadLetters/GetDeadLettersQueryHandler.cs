using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;

public sealed class GetDeadLettersQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetDeadLettersQuery, IReadOnlyList<DeadLetterMessageResponse>>
{
    public async Task<Result<IReadOnlyList<DeadLetterMessageResponse>>> Handle(GetDeadLettersQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetDeadLettersAsync(request.Page, request.PageSize, request.SourceModule, request.EventType, request.Status, cancellationToken));
}
