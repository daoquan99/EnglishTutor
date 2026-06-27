using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Application.Queries.GetUserSessions;

public sealed class GetUserSessionsQueryHandler
    : IQueryHandler<GetUserSessionsQuery, PagedResult<UserSessionResult>>
{
    private readonly IUserSessionQueryService _queryService;

    public GetUserSessionsQueryHandler(IUserSessionQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<PagedResult<UserSessionResult>>> Handle(
        GetUserSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await _queryService.GetActiveSessionsAsync(
            userId: request.UserId,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return Result.Success(page);
    }
}
