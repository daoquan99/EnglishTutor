using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Application.Users.Queries.ListPermissions;
using EnglishTutor.Identity.Application.Users.Queries.ListUsers;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;

namespace EnglishTutor.Identity.Application.Abstractions.Persistence;

public interface IUserManagementQueryService
{
    Task<PagedResult<UserListItemResult>> ListUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        string? role,
        CancellationToken cancellationToken);

    Task<UserDetailResult?> GetUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleResult>> ListRolesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<PermissionResult>> ListPermissionsAsync(CancellationToken cancellationToken);
}
