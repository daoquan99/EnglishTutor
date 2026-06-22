using EnglishTutor.BuildingBlocks.Domain.Aggregates;
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
    public Guid SessionId { get; private set; }
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
        Guid sessionId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId required.", nameof(userId));
        if (familyId == Guid.Empty) throw new ArgumentException("FamilyId required.", nameof(familyId));
        if (sessionId == Guid.Empty) throw new ArgumentException("SessionId required.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash required.", nameof(tokenHash));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            SessionId = sessionId,
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

    public void MarkReuseDetected(DateTime nowUtc, string reason)
    {
        ReuseDetectedAtUtc ??= nowUtc;
        RevokeFamily(nowUtc, reason);
    }

    public void RevokeFamily(DateTime nowUtc, string? reason)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }
        RevokedAtUtc = nowUtc;
        RaiseDomainEvent(new RefreshTokenReuseDetectedDomainEvent(
            UserId: UserId,
            SessionId: SessionId,
            RefreshTokenFamilyId: FamilyId,
            RefreshTokenId: Id,
            Reason: reason ?? "unknown"));
    }

    // Compatibility wrappers
    public void MarkUsed(Guid replacedByTokenId, DateTime nowUtc)
    {
        Consume(replacedByTokenId, nowUtc);
    }

    public void DetectReuse(string? ipAddress)
    {
        MarkReuseDetected(DateTime.UtcNow, ipAddress ?? "unknown");
    }
}
