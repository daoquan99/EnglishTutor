using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Audit.Application.Abstractions.Persistence;

namespace EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;

public sealed class SearchSecurityEventsQueryHandler
    : IQueryHandler<SearchSecurityEventsQuery, PagedResult<SecurityEventResponseDto>>
{
    private readonly IAuditQueryService _queryService;

    public SearchSecurityEventsQueryHandler(IAuditQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<PagedResult<SecurityEventResponseDto>>> Handle(
        SearchSecurityEventsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.SearchSecurityEventsAsync(request, cancellationToken);
        return Result.Success(result);
    }
}
