using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Application.Users.Queries.ListUsers;

public sealed class ListUsersHandler : IQueryHandler<ListUsersQuery, PagedResult<UserListItemResult>>
{
    private readonly IUserManagementQueryService _queryService;

    public ListUsersHandler(IUserManagementQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<PagedResult<UserListItemResult>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = await _queryService.ListUsersAsync(
            page: request.Page,
            pageSize: request.PageSize,
            search: request.Search,
            status: request.Status,
            role: request.Role,
            cancellationToken: cancellationToken);

        return Result.Success(page);
    }
}
