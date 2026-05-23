using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthRolePermissionRepository
{
    Task<IReadOnlyList<AuthPermission>> ListPermissionsAsync(bool includeDisabled, CancellationToken cancellationToken);

    Task<AuthPermission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken);

    Task<AuthPermission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken);

    Task AddPermissionAsync(AuthPermission permission, CancellationToken cancellationToken);

    Task<bool> IsPermissionAssignedAsync(Guid permissionId, CancellationToken cancellationToken);

    void RemovePermission(AuthPermission permission);

    Task<IReadOnlyList<AuthRole>> ListRolesAsync(bool includeDisabled, CancellationToken cancellationToken);

    Task<AuthRole?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<AuthRole?> GetRoleByNameAsync(string name, CancellationToken cancellationToken);

    Task AddRoleAsync(AuthRole role, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuthPermission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<AuthPermission>>> GetPermissionsByRoleIdsAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AuthRolePermission>> GetRolePermissionAssignmentsAsync(Guid roleId, CancellationToken cancellationToken);

    Task<bool> ArePermissionIdsValidAsync(IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken);

    Task<bool> AreRoleIdsValidAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);

    Task AddRolePermissionAsync(AuthRolePermission rolePermission, CancellationToken cancellationToken);

    void RemoveRolePermission(AuthRolePermission rolePermission);

    Task<bool> IsRoleAssignedToAnyUserAsync(Guid roleId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetUserIdsAssignedToRoleAsync(Guid roleId, CancellationToken cancellationToken);

    void RemoveRole(AuthRole role);
}
