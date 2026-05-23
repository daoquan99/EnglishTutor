using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid AuthUserId { get; private set; }
    public Guid SessionId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? LastUsedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    private RefreshToken() { }

    public static RefreshToken Create(Guid authUserId, Guid sessionId, string tokenHash, DateTime expiresAtUtc, DateTime utcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException("Auth user id is required.");
        }

        if (sessionId == Guid.Empty)
        {
            throw new DomainException("Auth session id is required.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainException("Refresh token hash is required.");
        }

        if (expiresAtUtc <= utcNow)
        {
            throw new DomainException("Refresh token expiry must be in the future.");
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            SessionId = sessionId,
            TokenHash = tokenHash.Trim(),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public void MarkUsed(DateTime utcNow)
    {
        if (!IsActive(utcNow))
        {
            throw new DomainException("Refresh token is not active.");
        }

        LastUsedAtUtc = utcNow;
    }

    public void Revoke(DateTime utcNow, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
