using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Application.Users.Queries.ListPermissions;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Application.Users.Queries.ListUsers;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Queries;

internal sealed class UserManagementQueryService : IUserManagementQueryService
{
    private readonly IdentityDbContext _db;

    public UserManagementQueryService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserListItemResult>> ListUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        string? role,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(user =>
                user.Email.Value.ToLower().Contains(term) ||
                user.DisplayName.ToLower().Contains(term));
        }

        query = NormalizeStatus(status) switch
        {
            UserStatusFilter.Active => query.Where(user => user.IsActive),
            UserStatusFilter.Inactive => query.Where(user => !user.IsActive),
            UserStatusFilter.Locked => query.Where(user => user.IsLockedOut),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleName = role.Trim();
            query =
                from user in query
                join userRole in _db.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                join roleRow in _db.Roles.AsNoTracking() on userRole.RoleId equals roleRow.Id
                where roleRow.Name == roleName
                select user;
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(user => user.Email.Value)
            .ThenBy(user => user.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(user => new UserProjection(
                Id: user.Id,
                Email: user.Email.Value,
                DisplayName: user.DisplayName,
                IsActive: user.IsActive,
                IsLockedOut: user.IsLockedOut,
                LockoutEndUtc: user.LockoutEndUtc,
                CreatedAtUtc: user.CreatedAtUtc,
                UpdatedAtUtc: user.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        var rolesByUserId = await LoadRolesByUserIdAsync(
            users.Select(user => user.Id).ToArray(),
            cancellationToken);

        var items = users
            .Select(user => new UserListItemResult(
                Id: user.Id,
                Email: user.Email,
                DisplayName: user.DisplayName,
                IsActive: user.IsActive,
                IsLockedOut: user.IsLockedOut,
                LockoutEndUtc: user.LockoutEndUtc,
                Roles: rolesByUserId.GetValueOrDefault(user.Id, []),
                CreatedAtUtc: user.CreatedAtUtc,
                UpdatedAtUtc: user.UpdatedAtUtc))
            .ToArray();

        return PagedResult<UserListItemResult>.Create(
            items: items,
            totalCount: totalCount,
            page: safePage,
            pageSize: safePageSize);
    }

    public async Task<UserDetailResult?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserDetailProjection(
                Id: user.Id,
                Email: user.Email.Value,
                DisplayName: user.DisplayName,
                IsActive: user.IsActive,
                IsLockedOut: user.IsLockedOut,
                LockoutEndUtc: user.LockoutEndUtc,
                FailedLoginAttempts: user.FailedLoginAttempts,
                CreatedAtUtc: user.CreatedAtUtc,
                UpdatedAtUtc: user.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var rolesByUserId = await LoadRolesByUserIdAsync([user.Id], cancellationToken);
        return new UserDetailResult(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            IsActive: user.IsActive,
            IsLockedOut: user.IsLockedOut,
            LockoutEndUtc: user.LockoutEndUtc,
            FailedLoginAttempts: user.FailedLoginAttempts,
            Roles: rolesByUserId.GetValueOrDefault(user.Id, []),
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc);
    }

    public async Task<IReadOnlyList<RoleResult>> ListRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .OrderByDescending(role => role.Priority)
            .ThenBy(role => role.Name)
            .Select(role => new RoleProjection(
                Id: role.Id,
                Name: role.Name,
                DisplayName: role.DisplayName,
                Priority: role.Priority))
            .ToListAsync(cancellationToken);

        var permissionCodesByRoleId = await LoadPermissionCodesByRoleIdAsync(
            roles.Select(role => role.Id).ToArray(),
            cancellationToken);

        return roles
            .Select(role => new RoleResult(
                Id: role.Id,
                Name: role.Name,
                DisplayName: role.DisplayName,
                Priority: role.Priority,
                PermissionCodes: permissionCodesByRoleId.GetValueOrDefault(role.Id, [])))
            .ToArray();
    }

    public async Task<IReadOnlyList<PermissionResult>> ListPermissionsAsync(CancellationToken cancellationToken)
    {
        return await _db.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.ModuleName)
            .ThenBy(permission => permission.Code)
            .Select(permission => new PermissionResult(
                Id: permission.Id,
                Code: permission.Code,
                ModuleName: permission.ModuleName,
                DisplayName: permission.DisplayName))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> LoadRolesByUserIdAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var rows = await (
                from userRole in _db.UserRoles.AsNoTracking()
                join role in _db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                orderby role.Priority descending, role.Name
                select new
                {
                    userRole.UserId,
                    role.Name
                })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(row => row.Name).ToArray());
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> LoadPermissionCodesByRoleIdAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var rows = await (
                from rolePermission in _db.RolePermissions.AsNoTracking()
                join permission in _db.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                where roleIds.Contains(rolePermission.RoleId)
                orderby permission.ModuleName, permission.Code
                select new
                {
                    rolePermission.RoleId,
                    permission.Code
                })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.RoleId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(row => row.Code).ToArray());
    }

    private static UserStatusFilter NormalizeStatus(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "active" => UserStatusFilter.Active,
            "inactive" => UserStatusFilter.Inactive,
            "locked" => UserStatusFilter.Locked,
            _ => UserStatusFilter.All
        };
    }

    private enum UserStatusFilter
    {
        All,
        Active,
        Inactive,
        Locked
    }

    private sealed record UserProjection(
        Guid Id,
        string Email,
        string DisplayName,
        bool IsActive,
        bool IsLockedOut,
        DateTime? LockoutEndUtc,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record UserDetailProjection(
        Guid Id,
        string Email,
        string DisplayName,
        bool IsActive,
        bool IsLockedOut,
        DateTime? LockoutEndUtc,
        int FailedLoginAttempts,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record RoleProjection(
        Guid Id,
        string Name,
        string DisplayName,
        int Priority);
}
