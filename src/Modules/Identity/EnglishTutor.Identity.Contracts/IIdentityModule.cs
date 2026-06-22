namespace EnglishTutor.Identity.Contracts;

/// <summary>
/// Public contract for cross-module identity access. Implemented by
/// <c>Identity.Application.Services.IdentityModuleService</c>. Consumed by
/// other modules (Learning, Practice, AiGateway, ...) to resolve users
/// and check permissions without referencing Identity internals.
/// </summary>
public interface IIdentityModule
{
    /// <summary>
    /// Returns a snapshot of the user (with roles and effective permissions),
    /// or <c>null</c> when the user is not found / inactive / soft-deleted.
    /// </summary>
    Task<UserSnapshot?> GetUserSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the user has the specified permission code.
    /// Returns <see cref="PermissionCheckResult.Allowed"/> = false when
    /// the user does not exist, is inactive, is locked out, or lacks the
    /// permission. The optional <see cref="PermissionCheckResult.Reason"/>
    /// explains why (for logging / audit; never expose to API callers).
    /// </summary>
    Task<PermissionCheckResult> CheckPermissionAsync(
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default);
}
