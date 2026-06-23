using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Audit.Application.Abstractions.Persistence;

namespace EnglishTutor.Audit.Application.Queries.SearchAuditLogs;

public sealed class SearchAuditLogsQueryHandler
    : IQueryHandler<SearchAuditLogsQuery, PagedResult<AuditLogResponseDto>>
{
    private readonly IAuditQueryService _queryService;

    public SearchAuditLogsQueryHandler(IAuditQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<PagedResult<AuditLogResponseDto>>> Handle(
        SearchAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.SearchAuditLogsAsync(request, cancellationToken);
        return Result.Success(result);
    }
}
