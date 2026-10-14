using System.Security.Cryptography;
using System.Text;
using EnglishLiveTutor.Application.Features.Identity;
using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Infrastructure.Identity;

public sealed class IdentityService(IDbContextFactory<AppDbContext> factory,
    IPasswordHasher<UserEntity> passwords, TimeProvider clock) : IIdentityService
{
    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static async Task<IdentityPrincipal?> Principal(AppDbContext db, Guid id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsEnabled, ct);
        if (user is null) return null;
        var roles = await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                           where ur.UserId == id && !r.IsDeleted select r.Name).Distinct().ToArrayAsync(ct);
        var permissions = await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                                 join rp in db.RolePermissions on r.Id equals rp.RoleId
                                 join p in db.Permissions on rp.PermissionId equals p.Id
                                 where ur.UserId == id && !r.IsDeleted && !p.IsDeleted
                                 select p.Code).Distinct().ToArrayAsync(ct);
        return new(user.Id, user.Email, user.DisplayName, roles, permissions);
    }

    public async Task<IdentityPrincipal?> RegisterAsync(string email, string displayName, string password, CancellationToken ct)
    {
        email = (email ?? string.Empty).Trim(); displayName = (displayName ?? string.Empty).Trim();
        password ??= string.Empty;
        if (email.Length is < 5 or > 256 || !email.Contains('@') || displayName.Length is < 1 or > 160 ||
            password.Length is < 12 or > 1024) throw new ArgumentException("Invalid email/name or password shorter than 12 characters.");
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Users.AnyAsync(x => x.NormalizedEmail == UserEntity.NormalizeEmail(email), ct)) return null;
        var user = new UserEntity(email, displayName, "");
        user.SetPasswordHash(passwords.HashPassword(user, password));
        var student = await db.Roles.SingleAsync(x => x.Name == "Student", ct);
        db.Users.Add(user); db.UserRoles.Add(new UserRoleEntity(user.Id, student.Id));
        await db.SaveChangesAsync(ct);
        return await Principal(db, user.Id, ct);
    }

    public async Task<IdentityPrincipal?> VerifyCredentialsAsync(string email, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) return null;
        await using var db = await factory.CreateDbContextAsync(ct);
        var normalized = UserEntity.NormalizeEmail(email);
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized && x.IsEnabled, ct);
        if (user is null || passwords.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed) return null;
        return await Principal(db, user.Id, ct);
    }

    public async Task<IdentityPrincipal?> FindUserAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await Principal(db, id, ct);
    }

    public async Task<(string RefreshToken, DateTimeOffset ExpiresAt)> CreateRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (!await db.Users.AnyAsync(x => x.Id == userId && x.IsEnabled, ct))
            throw new InvalidOperationException("Account is disabled or missing.");
        var token = Generate(); var expires = clock.GetUtcNow().AddDays(14);
        db.RefreshTokens.Add(new RefreshTokenEntity(userId, Hash(token), expires));
        await db.SaveChangesAsync(ct);
        return (token, expires);
    }

    public async Task<(IdentityPrincipal User, string RefreshToken, DateTimeOffset ExpiresAt)?> RotateRefreshTokenAsync(
        string presentedToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(presentedToken) || presentedToken.Length > 512) return null;
        await using var db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var old = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == Hash(presentedToken), ct);
        if (old is null) return null;
        var now = clock.GetUtcNow();
        if (old.RevokedAt is not null)
        {
            await db.RefreshTokens.Where(x => x.UserId == old.UserId && x.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
            await transaction.CommitAsync(ct);
            return null;
        }
        if (old.ExpiresAt <= now) return null;
        var principal = await Principal(db, old.UserId, ct);
        if (principal is null) return null;
        var token = Generate(); var expires = now.AddDays(14);
        var next = new RefreshTokenEntity(old.UserId, Hash(token), expires);
        var updated = await db.RefreshTokens.Where(x => x.Id == old.Id && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now)
                .SetProperty(x => x.ReplacedByTokenId, next.Id), ct);
        if (updated != 1) return null;
        db.RefreshTokens.Add(next); await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return (principal, token, expires);
    }

    public async Task RevokeRefreshTokenAsync(string presentedToken, Guid? expectedUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(presentedToken) || presentedToken.Length > 512) return;
        await using var db = await factory.CreateDbContextAsync(ct);
        var query = db.RefreshTokens.Where(x => x.TokenHash == Hash(presentedToken) && x.RevokedAt == null);
        if (expectedUserId.HasValue) query = query.Where(x => x.UserId == expectedUserId.Value);
        await query.ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, clock.GetUtcNow()), ct);
    }

    public async Task<IReadOnlyList<IdentityUserSummary>> ListUsersAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var users = await db.Users.AsNoTracking().OrderBy(x => x.Email).ToArrayAsync(ct);
        var memberships = await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                                 select new { ur.UserId, r.Name }).ToArrayAsync(ct);
        return users.Select(x => new IdentityUserSummary(x.Id, x.Email, x.DisplayName, x.IsEnabled,
            memberships.Where(y => y.UserId == x.Id).Select(y => y.Name).ToArray())).ToArray();
    }

    public async Task<IReadOnlyList<RoleSummary>> ListRolesAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var roles = await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToArrayAsync(ct);
        var mappings = await (from rp in db.RolePermissions join p in db.Permissions on rp.PermissionId equals p.Id
                              select new { rp.RoleId, p.Code }).ToArrayAsync(ct);
        return roles.Select(x => new RoleSummary(x.Id, x.Name, x.Description,
            mappings.Where(y => y.RoleId == x.Id).Select(y => y.Code).ToArray())).ToArray();
    }

    public async Task<IReadOnlyList<string>> ListPermissionsAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Permissions.OrderBy(x => x.Code).Select(x => x.Code).ToArrayAsync(ct);
    }

    private static async Task<bool> IsLastActiveAdminAsync(AppDbContext db, Guid userId, CancellationToken ct)
    {
        var adminRole = await db.Roles.SingleOrDefaultAsync(x => x.Name == "Admin", ct);
        if (adminRole is null || !await db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == adminRole.Id, ct))
            return false;
        var adminUsers = await (from membership in db.UserRoles
                                join u in db.Users on membership.UserId equals u.Id
                                where membership.RoleId == adminRole.Id && u.IsEnabled
                                select membership.UserId).Distinct().CountAsync(ct);
        return adminUsers <= 1;
    }

    public async Task<bool> SetUserEnabledAsync(Guid userId, bool enabled, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return false;
        if (!enabled && await IsLastActiveAdminAsync(db, userId, ct))
            throw new InvalidOperationException("Cannot disable the last active administrator.");
        user.SetEnabled(enabled);
        if (!enabled) await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, clock.GetUtcNow()), ct);
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetUserRoleAsync(Guid userId, string role, bool assign, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (!await db.Users.AnyAsync(x => x.Id == userId, ct)) return false;
        var target = await db.Roles.SingleOrDefaultAsync(x => x.Name == role.Trim(), ct);
        if (target is null) return false;
        var membership = await db.UserRoles.SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == target.Id, ct);
        if (assign && membership is null) db.UserRoles.Add(new UserRoleEntity(userId, target.Id));
        if (!assign && target.Name == "Admin" && await IsLastActiveAdminAsync(db, userId, ct))
            throw new InvalidOperationException("Cannot remove the last active administrator.");
        if (!assign && membership is not null) db.UserRoles.Remove(membership);
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct)
    {
        name = name.Trim(); description = description.Trim();
        if (name.Length is < 2 or > 64 || description.Length > 500)
            throw new ArgumentException("Invalid role name or description.");
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Roles.AnyAsync(x => x.Name == name, ct)) return false;
        db.Roles.Add(new RoleEntity(name, description));
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DeleteRoleAsync(string name, CancellationToken ct)
    {
        if (new[] { "Admin", "Teacher", "Student" }.Contains(name, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Built-in roles cannot be deleted.");
        await using var db = await factory.CreateDbContextAsync(ct);
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == name, ct);
        if (role is null) return false;
        role.SoftDelete(clock.GetUtcNow(), "system");
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> CreatePermissionAsync(string code, string description, CancellationToken ct)
    {
        code = code.Trim().ToLowerInvariant(); description = description.Trim();
        if (code.Length is < 3 or > 128 || !System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-z][a-z0-9._-]+$")
            || description.Length > 500)
            throw new ArgumentException("Invalid permission code or description.");
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Permissions.AnyAsync(x => x.Code == code, ct)) return false;
        db.Permissions.Add(new PermissionEntity(code, description));
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DeletePermissionAsync(string code, CancellationToken ct)
    {
        var builtIn = new[] { "users.read", "users.manage", "roles.read", "roles.manage",
            "prompts.read", "prompts.manage", "voices.manage" };
        if (builtIn.Contains(code, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Built-in permissions cannot be deleted.");
        await using var db = await factory.CreateDbContextAsync(ct);
        var permission = await db.Permissions.SingleOrDefaultAsync(x => x.Code == code, ct);
        if (permission is null) return false;
        permission.SoftDelete(clock.GetUtcNow(), "system");
        await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetRolePermissionAsync(string role, string permission, bool grant, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var r = await db.Roles.SingleOrDefaultAsync(x => x.Name == role.Trim(), ct);
        var p = await db.Permissions.SingleOrDefaultAsync(x => x.Code == permission.Trim().ToLowerInvariant(), ct);
        if (r is null || p is null) return false;
        if (r.Name == "Admin" && !grant)
            throw new InvalidOperationException("Cannot revoke built-in Admin permissions.");
        var link = await db.RolePermissions.SingleOrDefaultAsync(x => x.RoleId == r.Id && x.PermissionId == p.Id, ct);
        if (grant && link is null) db.RolePermissions.Add(new RolePermissionEntity(r.Id, p.Id));
        if (!grant && link is not null) db.RolePermissions.Remove(link);
        await db.SaveChangesAsync(ct); return true;
    }
}
