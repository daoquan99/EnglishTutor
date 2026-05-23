using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler(IAdminReportQueryService queryService)
    : IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogResponse>>
{
    public async Task<Result<IReadOnlyList<AuditLogResponse>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken) =>
        Result.Success(await queryService.GetAuditLogsAsync(request.Page, request.PageSize, request.Action, request.TargetEntity, cancellationToken));
}
