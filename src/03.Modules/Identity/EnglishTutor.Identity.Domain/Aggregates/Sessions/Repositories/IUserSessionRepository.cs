using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;

/// <summary>
/// Read-side snapshot returned by
/// <see cref="IUserSessionRepository.FindByActiveRefreshTokenHashAsync"/>:
/// the parent session, the family, and the single active token row that
/// matched the hash. The active token is filtered so only the relevant row
/// is loaded (not the full family history).
/// </summary>
public sealed record UserSessionWithActiveToken(
    UserSession Session,
    RefreshTokenFamily Family,
    RefreshToken ActiveToken);

/// <summary>
/// Repository abstraction for the <c>Sessions</c> aggregate. Persistence-
/// agnostic: NO Entity Framework infrastructure types are exposed in this
/// interface. EF Core types live only inside the Infrastructure
/// implementation. The unit-of-work interface is the only abstraction
/// that exposes a persistence-lifecycle call (see
/// <c>IIdentityUnitOfWork</c>).
/// </summary>
public interface IUserSessionRepository
{
    /// <summary>
    /// Two-step safe-mutation lookup by refresh-token hash.
    /// <para>
    /// The name intentionally does NOT contain "Active" — the lookup does
    /// NOT filter by active/consumed/revoked state. The caller (handler)
    /// inspects the returned snapshot to decide:
    /// <list type="bullet">
    ///   <item>consumed → reuse / theft detection</item>
    ///   <item>revoked or expired → invalid refresh token</item>
    ///   <item>active → rotate</item>
    /// </list>
    /// </para>
    /// <para>
    /// Step 1 — AsNoTracking projection: a lightweight query locates the
    /// matching refresh token's <c>Id</c> and parent <c>FamilyId</c> by
    /// hash. No tracked entities are returned from this step.
    /// </para>
    /// <para>
    /// Step 2 — Tracked aggregate load: the parent family and the single
    /// matching token are loaded as tracked EF entities (with a filtered
    /// Include so other historical tokens in the family are NOT loaded),
    /// then the parent session is loaded. The returned snapshot contains
    /// fully-tracked entities that the caller may safely mutate (e.g.
    /// <c>session.Revoke(...)</c>, <c>session.RotateRefreshToken(...)</c>).
    /// </para>
    /// <para>
    /// Returns <c>null</c> when the token is not found, or when the family
    /// or session rows referenced by the token are missing.
    /// </para>
    /// </summary>
    Task<UserSessionWithActiveToken?> FindByRefreshTokenHashAsync(
        RefreshTokenHash tokenHash,
        CancellationToken ct);

    /// <summary>
    /// Loads a single refresh token by its hash, regardless of state
    /// (active, consumed, revoked, expired). Returns <c>null</c> when no
    /// row matches.
    /// <para>
    /// Used by <c>RefreshCommandHandler</c> and <c>LogoutCommandHandler</c>
    /// while the auth runtime still operates without a backing
    /// <c>UserSession</c> / <c>RefreshTokenFamily</c> schema (those tables
    /// are introduced in Slice 2.7 alongside the EF migration). Once the
    /// session/family tables exist, callers should prefer
    /// <see cref="FindByActiveRefreshTokenHashAsync"/> which returns a
    /// tracked aggregate snapshot.
    /// </para>
    /// </summary>
    Task<RefreshToken?> FindByHashAsync(
        RefreshTokenHash tokenHash,
        CancellationToken ct);

    /// <summary>Loads a session by id. Excludes soft-deleted by default.</summary>
    Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken ct);

    /// <summary>Loads all active (non-revoked) sessions for a user.</summary>
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>Stages a new session for insertion.</summary>
    void Add(UserSession session);

    /// <summary>Stages a new refresh-token family for insertion.</summary>
    void Add(RefreshTokenFamily family);

    /// <summary>Stages a new refresh token for insertion.</summary>
    void Add(RefreshToken token);

    /// <summary>
    /// Purges all expired refresh tokens that expired before the threshold date.
    /// </summary>
    Task<int> PurgeExpiredRefreshTokensAsync(DateTime expiredBeforeUtc, CancellationToken cancellationToken);
}
