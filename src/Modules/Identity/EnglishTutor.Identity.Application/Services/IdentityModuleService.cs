using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Identity.Application.Services;

/// <summary>
/// Cross-module read-side implementation of <see cref="Contracts.IIdentityModule"/>.
/// Reads only — never mutates user state.
/// </summary>
public sealed class IdentityModuleService : Contracts.IIdentityModule
{
    private readonly IIdentityModuleDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<IdentityModuleService> _logger;

    public IdentityModuleService(
        IIdentityModuleDbContext db,
        ICurrentUser currentUser,
        ILogger<IdentityModuleService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Contracts.UserSnapshot?> GetUserSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.FindUserWithRolesAsync(userId, includeDeleted: true, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return null;
        }

        var roleIds = user.RoleIds.ToList();
        var roleNames = await _db.GetRoleNamesAsync(roleIds, cancellationToken);
        var permissions = await _db.GetPermissionCodesForRolesAsync(roleIds, cancellationToken);

        return new Contracts.UserSnapshot(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            roleNames,
            permissions,
            IsActive: true);
    }

    public async Task<Contracts.PermissionCheckResult> CheckPermissionAsync(
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetUserSnapshotAsync(userId, cancellationToken);
        if (snapshot is null)
        {
            return Contracts.PermissionCheckResult.Deny("user_not_found_or_inactive");
        }
        if (!snapshot.Permissions.Contains(permissionCode))
        {
            return Contracts.PermissionCheckResult.Deny("permission_not_granted");
        }
        return Contracts.PermissionCheckResult.Allow();
    }
}
