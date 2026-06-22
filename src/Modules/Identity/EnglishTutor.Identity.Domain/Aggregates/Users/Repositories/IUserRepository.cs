using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

/// <summary>
/// Repository abstraction for the <c>Users</c> aggregate. Persistence-agnostic:
/// NO Entity Framework infrastructure types are exposed in this interface.
/// EF Core types live only inside the Infrastructure implementation. The
/// unit-of-work interface is the only abstraction that exposes a
/// persistence-lifecycle call (see <c>IIdentityUnitOfWork</c>).
/// </summary>
public interface IUserRepository
{
    /// <summary>Loads a user by id (excluding soft-deleted by default).</summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Loads a user by id with explicit soft-delete control. Pass
    /// <paramref name="includeDeleted"/>=true to include soft-deleted rows
    /// (used by the refresh-token path which must continue to resolve a
    /// user even if the user has been soft-deleted between login and refresh).
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, bool includeDeleted, CancellationToken ct);

    /// <summary>
    /// Looks up a user by email. Set <paramref name="includeDeleted"/>=true
    /// to include soft-deleted rows (use only for admin / re-activation flows).
    /// </summary>
    Task<User?> FindByEmailAsync(Email email, bool includeDeleted, CancellationToken ct);

    /// <summary>Stages a new user for insertion. Persisted via the unit-of-work.</summary>
    void Add(User user);
}
