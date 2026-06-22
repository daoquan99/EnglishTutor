using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;

/// <summary>
/// Refresh token aggregate root. Each token belongs to a <see cref="FamilyId"/>:
/// the family starts when the user logs in, and rotation produces a new
/// token chained to the family via <see cref="ReplacedByTokenId"/>.
/// </summary>
/// <remarks>
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
    public Guid? ReplacedByTokenId { get; private set; }
    public string? CreatedByIp { get; private set; }

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

    /// <summary>
    /// Marks this token as used and chains it to its replacement. Throws
    /// <see cref="InvalidOperationException"/> if already used (caller should
    /// detect reuse and call <see cref="RevokeFamily"/> instead).
    /// </summary>
    public void MarkUsed(Guid replacedByTokenId, DateTime nowUtc)
    {
        if (UsedAtUtc is not null)
        {
            throw new InvalidOperationException("Refresh token already used.");
        }
        if (RevokedAtUtc is not null)
        {
            throw new InvalidOperationException("Refresh token revoked.");
        }
        UsedAtUtc = nowUtc;
        ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Revokes this token and raises a reuse-detection event.
    /// </summary>
    public void DetectReuse(string? ipAddress)
    {
        RevokeFamily(DateTime.UtcNow, ipAddress);
    }

    public void RevokeFamily(DateTime nowUtc, string? ipAddress)
    {
        RevokedAtUtc = nowUtc;
        RaiseDomainEvent(new RefreshTokenReuseDetectedDomainEvent(
            UserId, FamilyId, ipAddress));
    }
}
