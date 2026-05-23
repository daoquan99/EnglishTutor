using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListUsers;

public sealed class ListUsersQueryHandler(IAuthRepository repository)
    : IQueryHandler<ListUsersQuery, PagedResponse<AuthUserListItem>>
{
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public async Task<Result<PagedResponse<AuthUserListItem>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < MinPageSize => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize,
        };

        var (users, total) = await repository.ListAsync(
            request.Search,
            request.Status,
            request.RoleId,
            page,
            pageSize,
            request.Sort,
            cancellationToken);

        var items = users
            .Select(user => new AuthUserListItem(
                user.Id,
                user.Email.Value,
                user.DisplayName,
                user.IsActive,
                [.. user.Roles.Select(role => role.RoleId)],
                user.CreatedAtUtc,
                user.UpdatedAtUtc))
            .ToList();

        return new PagedResponse<AuthUserListItem>(items, total, page, pageSize);
    }
}
