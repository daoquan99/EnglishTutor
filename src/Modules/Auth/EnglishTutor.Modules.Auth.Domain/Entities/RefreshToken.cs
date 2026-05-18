using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid AuthUserId { get; private set; }
    public Guid SessionId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastUsedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid authUserId, Guid sessionId, string tokenHash, DateTime expiresAtUtc)
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

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            throw new DomainException("Refresh token expiry must be in the future.");
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            SessionId = sessionId,
            TokenHash = tokenHash.Trim(),
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkUsed()
    {
        if (!IsActive)
        {
            throw new DomainException("Refresh token is not active.");
        }

        LastUsedAtUtc = DateTime.UtcNow;
    }

    public void Revoke(Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
            return;

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
