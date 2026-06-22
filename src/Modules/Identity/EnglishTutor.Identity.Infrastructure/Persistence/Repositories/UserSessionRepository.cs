using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserSessionRepository"/>. Because
/// Slice 2.5 adds no <c>DbSet</c> properties to <c>IdentityDbContext</c>
/// (EF configurations are deferred to Slice 2.7), this repository uses
/// <c>_db.Set&lt;T&gt;()</c> for <c>UserSession</c> and
/// <c>RefreshTokenFamily</c>. <c>RefreshToken</c> continues to use
/// <c>_db.RefreshTokens</c>.
/// </summary>
internal sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly IdentityDbContext _db;

    public UserSessionRepository(IdentityDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Two-step safe-mutation lookup:
    /// (a) AsNoTracking projection to locate the matching token's id and
    ///     its parent family id by refresh-token hash (no tracked entities);
    /// (b) load the family with a filtered Include so only the matching
    ///     token row is loaded, then load the parent session. All entities
    ///     returned in the snapshot are tracked so the caller can safely
    ///     invoke domain methods that mutate them (e.g.
    ///     <c>UserSession.RotateRefreshToken</c>,
    ///     <c>RefreshToken.MarkReuseDetected</c>,
    ///     <c>UserSession.Revoke</c>).
    /// </summary>
    public async Task<UserSessionWithActiveToken?> FindByRefreshTokenHashAsync(
        RefreshTokenHash tokenHash, CancellationToken ct)
    {
        // Step 1: AsNoTracking projection. Find the token id and family id.
        // No state filter — consumed/revoked tokens are returned so the
        // caller can perform reuse / theft detection.
        var match = await _db.RefreshTokens
            .AsNoTracking()
            .Where(t => t.TokenHash == tokenHash.Hex)
            .Select(t => new { TokenId = t.Id, FamilyId = t.FamilyId })
            .FirstOrDefaultAsync(ct);

        if (match is null)
        {
            return null;
        }

        // Step 2: load the family with ONLY the matching token (filtered Include).
        var family = await _db.Set<RefreshTokenFamily>()
            .Include(f => f.Tokens.Where(t => t.Id == match.TokenId))
            .FirstOrDefaultAsync(f => f.Id == match.FamilyId, ct);

        if (family is null)
        {
            return null;
        }

        // Step 2b: load the parent session (tracked) for safe mutation.
        var session = await _db.Set<UserSession>()
            .FirstOrDefaultAsync(s => s.Id == family.SessionId, ct);

        if (session is null)
        {
            return null;
        }

        var activeToken = family.Tokens.First();
        return new UserSessionWithActiveToken(session, family, activeToken);
    }

    /// <summary>
    /// Loads a single refresh token by its hash, ignoring query filters so
    /// consumed / revoked / expired rows are visible. Used by handlers
    /// while the auth runtime operates without a backing UserSession /
    /// RefreshTokenFamily schema.
    /// </summary>
    public Task<RefreshToken?> FindByHashAsync(
        RefreshTokenHash tokenHash, CancellationToken ct) =>
        _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash.Hex, ct);

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken ct) =>
        _db.Set<UserSession>().FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public void Add(UserSession session) => _db.Set<UserSession>().Add(session);

    public void Add(RefreshTokenFamily family) => _db.Set<RefreshTokenFamily>().Add(family);

    public void Add(RefreshToken token) => _db.RefreshTokens.Add(token);
}
