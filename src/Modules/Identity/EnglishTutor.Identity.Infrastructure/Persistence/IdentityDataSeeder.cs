using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Domain;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Infrastructure.Persistence.Seed.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeds the Identity lookup data (permissions, roles, role-permissions)
/// and the Owner user account on first startup.
/// Idempotent — running it twice does not duplicate data.
/// <para>
/// <b>Seed order (required for FK correctness):</b>
/// <list type="number">
///   <item><c>SeedPermissionsAsync</c> — permission codes</item>
///   <item><c>SeedRolesAsync</c> — well-known roles</item>
///   <item><c>SeedRolePermissionsAsync</c> — role-to-permission mapping</item>
///   <item><c>SaveChangesAsync</c> — persist 1, 2, 3 so subsequent
///         queries can find them</item>
///   <item><c>SeedOwnerAsync</c> — Owner user</item>
///   <item><c>SeedOwnerUserRoleAsync</c> — Owner user-to-role mapping</item>
///   <item><c>SaveChangesAsync</c> — final flush</item>
/// </list>
/// </para>
/// </summary>
public sealed class IdentityDataSeeder
{
    // Permission codes are module-scoped strings. We seed a small baseline
    // here so role-permission lookups always have something to find.
    private static readonly (string Code, string Module, string DisplayName)[] BaselinePermissions =
    {
        (IdentityPermissionCodes.Manage,     IdentityModuleNames.Identity, "Manage identity (create/update/delete users)"),
        (IdentityPermissionCodes.View,       IdentityModuleNames.Identity, "View identity data (users, roles)"),
        (IdentityPermissionCodes.SelfManage, IdentityModuleNames.Identity, "Manage own account (profile, password)"),
    };

    // (Role name, permission codes assigned to that role).
    private static readonly (string Role, string[] Permissions)[] BaselineRolePermissions =
    {
        (Role.WellKnownNames.Owner, new[] { IdentityPermissionCodes.Manage, IdentityPermissionCodes.View, IdentityPermissionCodes.SelfManage }),
        (Role.WellKnownNames.Admin, new[] { IdentityPermissionCodes.Manage, IdentityPermissionCodes.View, IdentityPermissionCodes.SelfManage }),
        (Role.WellKnownNames.User,  new[] { IdentityPermissionCodes.SelfManage }),
    };

    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedDataOptions _options;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        IdentityDbContext db,
        IPasswordHasher passwordHasher,
        IOptions<SeedDataOptions> options,
        IDateTimeProvider clock,
        ILogger<IdentityDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // 1-3. Stage permissions, roles, and the role-permission join rows.
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedRolePermissionsAsync(cancellationToken);

        // 4. Persist permissions + roles + role_permissions so subsequent
        //    queries (and FK inserts from UserRole) can resolve their Ids.
        //    Without this flush, any query against the just-Added entities
        //    hits the database (not the change tracker) and returns nothing.
        await _db.SaveChangesAsync(cancellationToken);

        // 5-7. Owner user + Owner UserRole join row + final flush.
        await SeedOwnerAsync(cancellationToken);
        await SeedOwnerUserRoleAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        foreach (var (code, module, display) in BaselinePermissions)
        {
            var exists = await _db.Permissions
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Code == code, ct);
            if (!exists)
            {
                _db.Permissions.Add(new Permission(code, module, display));
            }
        }
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.Owner, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.Owner, "System Owner", priority: 100));
        }
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.Admin, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.Admin, "Administrator", priority: 50));
        }
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.User, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.User, "Standard User", priority: 10));
        }
    }

    private async Task SeedRolePermissionsAsync(CancellationToken ct)
    {
        // Materialise current role + permission lookup once; cheaper than N+1.
        var rolesByName = await _db.Roles.IgnoreQueryFilters()
            .ToDictionaryAsync(r => r.Name, ct);
        var permissionsByCode = await _db.Permissions.IgnoreQueryFilters()
            .ToDictionaryAsync(p => p.Code, ct);

        foreach (var (roleName, permissionCodes) in BaselineRolePermissions)
        {
            if (!rolesByName.TryGetValue(roleName, out var role))
            {
                // Should not happen — SeedRolesAsync just ran above.
                _logger.LogWarning("Role {Role} missing while seeding role permissions; skipping.", roleName);
                continue;
            }

            foreach (var code in permissionCodes)
            {
                if (!permissionsByCode.TryGetValue(code, out var permission))
                {
                    _logger.LogWarning("Permission {Code} missing while seeding role permissions; skipping.", code);
                    continue;
                }

                var exists = await _db.RolePermissions.IgnoreQueryFilters().AnyAsync(
                    rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, ct);
                if (!exists)
                {
                    _db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                    });
                }
            }
        }
    }

    private async Task SeedOwnerAsync(CancellationToken ct)
    {
        var ownerEmail = _options.Owner.Email.Trim().ToLowerInvariant();

        // Existence check via EF Core LINQ on the owned Email navigation.
        // EF translates u.Email.Value to a SQL predicate on the owned column;
        // AnyAsync returns bool without ever materialising the User entity, so
        // the User.Email getter (which throws when the EF rehydrator has not
        // yet populated the backing field) is never invoked.
        if (await OwnerUserExistsAsync(ownerEmail, ct))
        {
            return;
        }

        // Dev default: random password (logged ONCE so dev can copy it). In production
        // the password MUST be supplied via SeedData:Owner:Password configuration.
        var password = string.IsNullOrWhiteSpace(_options.Owner.Password)
            ? GenerateRandomPassword()
            : _options.Owner.Password;

        // Use FirstOrDefault + defensive create. We do NOT use FirstAsync here
        // because: (a) the Owner role is staged above and persisted before this
        // method runs, so it MUST exist — but if it ever doesn't (e.g. someone
        // calls SeedAsync from a partial state), we want a clean retry signal,
        // not a SingleAsync / FirstAsync crash.
        var ownerRole = await _db.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == Role.WellKnownNames.Owner, ct);
        if (ownerRole is null)
        {
            ownerRole = new Role(Role.WellKnownNames.Owner, "System Owner", priority: 100);
            _db.Roles.Add(ownerRole);
            await _db.SaveChangesAsync(ct);
        }

        var user = User.Create(
            email: Email.Create(ownerEmail),
            passwordHash: HashedPassword.FromNewHash(_passwordHasher.HashPassword(password)),
            displayName: _options.Owner.DisplayName,
            roleIds: new[] { ownerRole.Id },
            createdByUserId: null);

        _db.Users.Add(user);

        _logger.LogWarning(
            "Seeded Owner account. Email: {Email}. Initial password: {Password}. " +
            "Rotate this password IMMEDIATELY in production environments.",
            ownerEmail, password);
    }

    private async Task SeedOwnerUserRoleAsync(CancellationToken ct)
    {
        var ownerEmail = _options.Owner.Email.Trim().ToLowerInvariant();

        // Project straight to the user Id — EF never materialises the User
        // entity, so the User.Email getter is never invoked.
        var ownerUserId = await _db.Users.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => u.Email.Value == ownerEmail)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(ct);
        if (ownerUserId is null)
        {
            return; // no owner yet — nothing to link.
        }

        var ownerRoleId = await _db.Roles.IgnoreQueryFilters()
            .Where(r => r.Name == Role.WellKnownNames.Owner)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(ct);
        if (ownerRoleId is null)
        {
            return; // no owner role yet — nothing to link.
        }

        var exists = await _db.UserRoles.IgnoreQueryFilters().AnyAsync(
            ur => ur.UserId == ownerUserId.Value && ur.RoleId == ownerRoleId.Value, ct);
        if (!exists)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = ownerUserId.Value,
                RoleId = ownerRoleId.Value,
            });
        }
    }

    private Task<bool> OwnerUserExistsAsync(string normalizedEmail, CancellationToken ct)
    {
        return _db.Users.AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.Value == normalizedEmail, ct);
    }

    private string GenerateRandomPassword()
    {
        const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
