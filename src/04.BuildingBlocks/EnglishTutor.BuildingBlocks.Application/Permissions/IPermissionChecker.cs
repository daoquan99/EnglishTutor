namespace EnglishTutor.BuildingBlocks.Application.Permissions;

/// <summary>
/// Abstraction for checking user permissions and roles.
/// Implemented by the Identity module to query roles and permissions from the database.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks whether a user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user is in a specific role.
    /// </summary>
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all permissions for a user (used for permission version checks).
    /// </summary>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
