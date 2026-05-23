using EnglishTutor.Modules.Auth.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthPermissionRepository(AuthDbContext dbContext) : IAuthPermissionRepository
{
    public async Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query =
            from userRole in dbContext.AuthUserRoles
            join role in dbContext.AuthRoles on userRole.RoleId equals role.Id
            join rolePermission in dbContext.AuthRolePermissions on role.Id equals rolePermission.RoleId
            join permission in dbContext.AuthPermissions on rolePermission.PermissionId equals permission.Id
            where userRole.AuthUserId == userId && role.IsEnabled && permission.IsEnabled
            select permission.Code;

        return await query
            .Distinct()
            .OrderBy(code => code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query =
            from userRole in dbContext.AuthUserRoles
            join role in dbContext.AuthRoles on userRole.RoleId equals role.Id
            where userRole.AuthUserId == userId && role.IsEnabled
            select role.Name;

        return await query
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);
    }
}
