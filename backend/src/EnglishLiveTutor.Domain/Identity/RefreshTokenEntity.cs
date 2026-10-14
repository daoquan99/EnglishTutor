using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

/// <summary>Only SHA-256 hashes of opaque refresh tokens are ever persisted.</summary>
public sealed class RefreshTokenEntity : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = "";
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    private RefreshTokenEntity() { }
    public RefreshTokenEntity(Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid(); UserId = userId;
        TokenHash = tokenHash; ExpiresAt = expiresAt;
    }
    public void Revoke(DateTimeOffset now, Guid? replacedByTokenId = null)
    {
        if (RevokedAt is not null) return;
        RevokedAt = now; ReplacedByTokenId = replacedByTokenId;
    }
}
