using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRoleRepository"/>. Uses the existing
/// <c>DbSet&lt;Role&gt; Roles</c>, <c>DbSet&lt;RolePermission&gt;
/// RolePermissions</c>, and <c>DbSet&lt;Permission&gt; Permissions</c>
/// properties. <c>UserSession</c> and <c>RefreshTokenFamily</c> are
/// accessed via <c>_db.Set&lt;T&gt;()</c> in
/// <c>UserSessionRepository</c> (no <c>DbSet</c> properties added in this
/// slice per the plan).
/// </summary>
internal sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _db;

    public RoleRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

    public Task<Role?> GetByNameAsync(string name, CancellationToken ct) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct) =>
        await _db.Roles.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetPermissionCodesForRolesAsync(
        IReadOnlyCollection<Guid> roleIds, CancellationToken ct)
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
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetNamesByIdsAsync(
        IReadOnlyCollection<Guid> roleIds, CancellationToken ct)
    {
        if (roleIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        return await _db.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(ct);
    }

    public void Add(Role role) => _db.Roles.Add(role);
}
