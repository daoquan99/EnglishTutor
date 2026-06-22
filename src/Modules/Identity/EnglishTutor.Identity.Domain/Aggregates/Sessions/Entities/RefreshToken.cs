using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;

/// <summary>
/// Refresh token entity. Each token belongs to a <see cref="FamilyId"/>:
/// the family starts when the user logs in, and rotation produces a new
/// token chained to the family via <see cref="ReplacedByTokenId"/>.
/// </summary>
/// <remarks>
/// <para>This is a child entity of <c>RefreshTokenFamily</c> (which is a
/// child entity of <c>UserSession</c>). It is NOT an <c>AggregateRoot</c>
/// because its lifecycle is owned by the family/session.</para>
/// <para>Reuse detection: if a token whose <see cref="UsedAtUtc"/> is already
/// set is presented again, the entire family is revoked
/// (<see cref="RevokeFamily"/>) — this protects against token-theft.</para>
/// <para>Plaintext token value is never persisted — only its SHA-256 hash.</para>
/// </remarks>
public sealed class RefreshToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime? ReuseDetectedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? CreatedByIp { get; private set; }

    /// <summary>True when this token has been consumed at least once.</summary>
    public bool IsConsumed => UsedAtUtc is not null;

    // EF Core parameterless constructor.
    private RefreshToken() { }

    public static RefreshToken Issue(
        Guid userId,
        Guid familyId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId required.", nameof(userId));
        if (familyId == Guid.Empty) throw new ArgumentException("FamilyId required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash required.", nameof(tokenHash));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedByIp = createdByIp
        };
    }

    /// <summary>True when this token can still be presented to issue a new one.</summary>
    public bool IsActive() =>
        RevokedAtUtc is null && UsedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    // ---- New explicit methods (Slice 2.4) ----

    /// <summary>
    /// Marks this token as consumed and chains it to the supplied replacement
    /// token id. Throws <see cref="InvalidOperationException"/> if the token
    /// has already been consumed or has been revoked.
    /// </summary>
    public void Consume(Guid replacementTokenId, DateTime nowUtc)
    {
        if (UsedAtUtc is not null)
        {
            throw new InvalidOperationException("Refresh token already consumed.");
        }
        if (RevokedAtUtc is not null)
        {
            throw new InvalidOperationException("Refresh token revoked.");
        }
        UsedAtUtc = nowUtc;
        ReplacedByTokenId = replacementTokenId;
    }

    /// <summary>
    /// Marks reuse detection: sets <see cref="ReuseDetectedAtUtc"/>, then
    /// calls <see cref="RevokeFamily"/> with the supplied reason to revoke
    /// the family. Idempotent for the timestamp but always revokes the
    /// family once per call.
    /// </summary>
    public void MarkReuseDetected(DateTime nowUtc, string reason)
    {
        ReuseDetectedAtUtc ??= nowUtc;
        RevokeFamily(nowUtc, reason);
    }

    /// <summary>
    /// Revokes this token and raises a reuse-detection event. The
    /// <paramref name="reason"/> is recorded on the family. Used by
    /// <c>RefreshTokenFamily.Revoke</c> when the family is revoked, and
    /// by <see cref="MarkReuseDetected"/> on detected reuse.
    /// </summary>
    public void RevokeFamily(DateTime nowUtc, string? reason)
    {
        RevokedAtUtc = nowUtc;
        RaiseDomainEvent(new RefreshTokenReuseDetectedDomainEvent(
            UserId, FamilyId, reason));
    }

    // ---- Compatibility wrappers (kept for current handler callers) ----

    /// <summary>
    /// Compatibility wrapper. Delegates to <see cref="Consume"/>. Existing
    /// handlers in Slice 2.4 still call this; it will be removed in
    /// Slice 2.6 when handlers are refactored.
    /// </summary>
    public void MarkUsed(Guid replacedByTokenId, DateTime nowUtc)
    {
        Consume(replacedByTokenId, nowUtc);
    }

    /// <summary>
    /// Compatibility wrapper. Delegates to <see cref="MarkReuseDetected"/>
    /// with the current UTC time and the supplied IP as the reason.
    /// </summary>
    public void DetectReuse(string? ipAddress)
    {
        MarkReuseDetected(DateTime.UtcNow, ipAddress ?? "unknown");
    }
}
