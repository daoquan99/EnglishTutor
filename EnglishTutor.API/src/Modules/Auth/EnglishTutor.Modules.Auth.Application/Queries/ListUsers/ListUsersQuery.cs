using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListUsers;

public sealed record ListUsersQuery(
    string? Search,
    UserStatusFilter Status,
    Guid? RoleId,
    int Page,
    int PageSize,
    UserSortOrder Sort = UserSortOrder.NewestFirst) : IQuery<PagedResponse<AuthUserListItem>>;
