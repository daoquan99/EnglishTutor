using EnglishTutor.Identity.Domain.Aggregates.Users;

namespace EnglishTutor.Identity.Application.Services;

/// <summary>
/// Narrow repository abstraction used by <see cref="IdentityModuleService"/>
/// so the service depends on a small contract instead of the full
/// <c>IdentityDbContext</c>. Implemented in Infrastructure.
/// </summary>
public interface IIdentityModuleDbContext
{
    Task<User?> FindUserWithRolesAsync(
        Guid userId,
        bool includeDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetPermissionCodesForRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken);
}
