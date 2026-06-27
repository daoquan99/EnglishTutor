namespace EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;

/// <summary>
/// Repository abstraction for the <c>Roles</c> aggregate. Persistence-
/// agnostic: NO Entity Framework infrastructure types are exposed in this
/// interface. EF Core types live only inside the Infrastructure
/// implementation. The unit-of-work interface is the only abstraction
/// that exposes a persistence-lifecycle call (see
/// <c>IIdentityUnitOfWork</c>).
/// </summary>
public interface IRoleRepository
{
    /// <summary>Loads a role by id.</summary>
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct);

    /// <summary>Loads a role by name (e.g. "Owner", "Admin", "User").</summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken ct);

    /// <summary>Lists all roles (for admin / cross-module discovery).</summary>
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct);

    /// <summary>
    /// Returns the union of permission codes granted to the supplied role ids.
    /// Cross-module consumers (Audit, Realtime, etc.) should call this
    /// instead of reading the Permission table directly.
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionCodesForRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct);

    /// <summary>
    /// Returns the display names of the supplied role ids. Used by handlers
    /// that need to embed role names in issued JWT claims (login, refresh,
    /// current-user lookup).
    /// </summary>
    Task<IReadOnlyList<string>> GetNamesByIdsAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct);

    /// <summary>Stages a new role for insertion.</summary>
    void Add(Role role);
}
