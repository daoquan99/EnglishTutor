using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;

// Refresh token entity. Each token belongs to a FamilyId:
// the family starts when the user logs in, and rotation produces a new
// token chained to the family via ReplacedByTokenId.
//
// This is a child entity of RefreshTokenFamily (which is a
// child entity of UserSession). It is NOT an AggregateRoot
// because its lifecycle is owned by the family and session.
//
// The session id is NOT persisted on this entity. It is available
// through RefreshTokenFamily.SessionId -> UserSession.Id.
// When the refresh-token-reuse event is raised, the session id is
// passed in as a method parameter (see MarkReuseDetected).
//
// Reuse detection: if a token whose UsedAtUtc is already
// set is presented again, the entire family is revoked
// (see RevokeFamily) — this protects against token-theft.
//
// Plaintext token value is never persisted — only its SHA-256 hash.
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

    public bool IsActive() =>
        RevokedAtUtc is null && UsedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

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

    // Marks reuse detection: sets ReuseDetectedAtUtc, then calls
    // RevokeFamily with the supplied reason to revoke the family.
    // The sessionId is NOT persisted; it is passed through to the
    // raised RefreshTokenReuseDetectedDomainEvent so the audit pipeline
    // can record the session context without a schema change on this
    // entity.
    public void MarkReuseDetected(DateTime nowUtc, string reason, Guid sessionId)
    {
        ReuseDetectedAtUtc ??= nowUtc;
        RevokeFamily(nowUtc, reason, sessionId);
    }

    // Revokes this token and raises a reuse-detection event. The
    // sessionId is the runtime context id (NOT a persisted property on
    // this token) used for the raised event. Idempotent: if the token
    // is already revoked, the call is a no-op.
    public void RevokeFamily(DateTime nowUtc, string? reason, Guid sessionId)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }
        RevokedAtUtc = nowUtc;
        RaiseDomainEvent(new RefreshTokenReuseDetectedDomainEvent(
            UserId: UserId,
            SessionId: sessionId,
            RefreshTokenFamilyId: FamilyId,
            RefreshTokenId: Id,
            Reason: reason ?? "unknown"));
    }

    // Compatibility wrapper. Delegates to Consume. Existing
    // handlers in Slice 2.4 still call this; it will be removed in
    // Slice 2.6 when handlers are refactored.
    public void MarkUsed(Guid replacedByTokenId, DateTime nowUtc)
    {
        Consume(replacedByTokenId, nowUtc);
    }
}
