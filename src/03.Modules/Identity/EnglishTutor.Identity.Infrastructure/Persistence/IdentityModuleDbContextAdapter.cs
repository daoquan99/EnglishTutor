using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

/// <summary>
/// Adapter that exposes the read-side queries <see cref="IdentityModuleService"/>
/// needs without leaking the full <see cref="IdentityDbContext"/> type.
/// </summary>
internal sealed class IdentityModuleDbContextAdapter : IIdentityModuleDbContext
{
    private readonly IdentityDbContext _db;

    public IdentityModuleDbContextAdapter(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindUserWithRolesAsync(
        Guid userId,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return Array.Empty<string>();
        }
        return await _db.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(_db.Permissions.AsNoTracking(),
                rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
