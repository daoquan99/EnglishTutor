using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Users.Queries.ListUsers;

public sealed record ListUsersQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Role) : IQuery<PagedResult<UserListItemResult>>;
