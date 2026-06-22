using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;

/// <summary>
/// A chain of rotated refresh tokens created by one login. If any consumed
/// token is reused, the entire family is revoked (token-theft protection).
/// This is a child entity of <c>UserSession</c> (Sessions aggregate), not
/// an <c>AggregateRoot</c> — its lifecycle is owned by the session.
/// </summary>
/// <remarks>
/// Plain class (no audit metadata) because family rows are append-only
/// and short-lived: they are revoked when the session ends. We do not add
/// soft-delete per Slice 2.4 plan.
/// </remarks>
public sealed class RefreshTokenFamily
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SessionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedReason { get; private set; }

    private readonly List<RefreshToken> _tokens = new();
    public IReadOnlyList<RefreshToken> Tokens => _tokens.AsReadOnly();

    // EF Core parameterless constructor.
    private RefreshTokenFamily() { }

    /// <summary>
    /// Factory: creates a new family bound to a freshly-created
    /// <c>UserSession</c>. The family is created with no tokens — the
    /// first refresh token is issued separately and added via
    /// <see cref="AddToken"/>.
    /// </summary>
    public static RefreshTokenFamily Create(Guid userId, Guid sessionId, DateTime nowUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId required.", nameof(userId));
        if (sessionId == Guid.Empty) throw new ArgumentException("SessionId required.", nameof(sessionId));

        return new RefreshTokenFamily
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            CreatedAtUtc = nowUtc
        };
    }

    /// <summary>Appends a token to the family. Used by the rotation flow.</summary>
    public void AddToken(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        _tokens.Add(token);
    }

    /// <summary>
    /// Revokes the family. All tokens in the family are also revoked. Idempotent:
    /// calling twice does not raise a second event.
    /// </summary>
    public void Revoke(DateTime nowUtc, string reason)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = nowUtc;
        RevokedReason = reason;

        foreach (var token in _tokens)
        {
            token.RevokeFamily(nowUtc, reason);
        }
    }

    public bool IsRevoked => RevokedAtUtc is not null;
}
