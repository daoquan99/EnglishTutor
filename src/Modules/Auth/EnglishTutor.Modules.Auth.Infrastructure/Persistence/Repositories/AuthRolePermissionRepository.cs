using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthRolePermissionRepository(AuthDbContext dbContext) : IAuthRolePermissionRepository
{
    public async Task<IReadOnlyList<AuthPermission>> ListPermissionsAsync(bool includeDisabled, CancellationToken cancellationToken)
    {
        var query = dbContext.AuthPermissions.AsQueryable();

        if (!includeDisabled)
        {
            query = query.Where(permission => permission.IsEnabled);
        }

        return await query
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<AuthPermission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken) =>
        dbContext.AuthPermissions.SingleOrDefaultAsync(permission => permission.Id == permissionId, cancellationToken);

    public Task<AuthPermission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();
        return dbContext.AuthPermissions.SingleOrDefaultAsync(permission => permission.Code == normalizedCode, cancellationToken);
    }

    public async Task AddPermissionAsync(AuthPermission permission, CancellationToken cancellationToken) =>
        await dbContext.AuthPermissions.AddAsync(permission, cancellationToken);

    public Task<bool> IsPermissionAssignedAsync(Guid permissionId, CancellationToken cancellationToken) =>
        dbContext.AuthRolePermissions.AnyAsync(rolePermission => rolePermission.PermissionId == permissionId, cancellationToken);

    public void RemovePermission(AuthPermission permission) =>
        dbContext.AuthPermissions.Remove(permission);

    public async Task<IReadOnlyList<AuthRole>> ListRolesAsync(bool includeDisabled, CancellationToken cancellationToken)
    {
        var query = dbContext.AuthRoles.AsQueryable();

        if (!includeDisabled)
        {
            query = query.Where(role => role.IsEnabled);
        }

        return await query
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<AuthRole?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken) =>
        dbContext.AuthRoles.SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);

    public Task<AuthRole?> GetRoleByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return dbContext.AuthRoles
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(role => role.Name == normalizedName, cancellationToken);
    }

    public async Task AddRoleAsync(AuthRole role, CancellationToken cancellationToken) =>
        await dbContext.AuthRoles.AddAsync(role, cancellationToken);

    public async Task<IReadOnlyList<AuthPermission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var query =
            from rolePermission in dbContext.AuthRolePermissions
            join permission in dbContext.AuthPermissions on rolePermission.PermissionId equals permission.Id
            where rolePermission.RoleId == roleId
            select permission;

        return await query
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<AuthPermission>>> GetPermissionsByRoleIdsAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var assignments = await (
            from rolePermission in dbContext.AuthRolePermissions
            join permission in dbContext.AuthPermissions on rolePermission.PermissionId equals permission.Id
            where roleIds.Contains(rolePermission.RoleId)
            orderby permission.Code
            select new { rolePermission.RoleId, Permission = permission })
            .ToListAsync(cancellationToken);

        return assignments
            .GroupBy(assignment => assignment.RoleId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<AuthPermission>)group.Select(assignment => assignment.Permission).ToList());
    }

    public async Task<IReadOnlyList<AuthRolePermission>> GetRolePermissionAssignmentsAsync(
        Guid roleId,
        CancellationToken cancellationToken) =>
        await dbContext.AuthRolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ArePermissionIdsValidAsync(IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return true;
        }

        var distinctPermissionIds = permissionIds.Distinct().ToArray();
        var existingCount = await dbContext.AuthPermissions
            .CountAsync(
                permission => distinctPermissionIds.Contains(permission.Id) && permission.IsEnabled,
                cancellationToken);

        return existingCount == distinctPermissionIds.Length;
    }

    public async Task AddRolePermissionAsync(AuthRolePermission rolePermission, CancellationToken cancellationToken) =>
        await dbContext.AuthRolePermissions.AddAsync(rolePermission, cancellationToken);

    public void RemoveRolePermission(AuthRolePermission rolePermission) =>
        dbContext.AuthRolePermissions.Remove(rolePermission);

    public Task<bool> IsRoleAssignedToAnyUserAsync(Guid roleId, CancellationToken cancellationToken) =>
        dbContext.AuthUserRoles.AnyAsync(userRole => userRole.RoleId == roleId, cancellationToken);

    public void RemoveRole(AuthRole role) =>
        dbContext.AuthRoles.Remove(role);
}
