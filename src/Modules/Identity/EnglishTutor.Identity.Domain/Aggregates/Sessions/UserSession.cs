using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions;

/// <summary>
/// Aggregate root for the Sessions aggregate. Represents one server-side
/// login session: device binding, refresh-token family, last-seen, and
/// revocation state. Soft-delete and audit metadata come from
/// <see cref="AggregateRoot"/>.
/// </summary>
public sealed class UserSession : AggregateRoot
{
    public Guid UserId { get; private set; }
    public DeviceInfo Device { get; private set; } = null!; // EF Core sets
    // CreatedAtUtc, UpdatedAtUtc, CreatedByUserId, UpdatedByUserId come from AggregateRoot/Entity.
    public DateTime LastSeenAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedReason { get; private set; }

    private RefreshTokenFamily? _family;
    public RefreshTokenFamily? Family => _family;

    // EF Core parameterless constructor.
    private UserSession() { }

    /// <summary>
    /// Factory: creates a new <c>UserSession</c> and a bound
    /// <c>RefreshTokenFamily</c>. Raises <c>UserSessionCreatedDomainEvent</c>.
    /// </summary>
    public static UserSession Create(Guid userId, DeviceInfo device, DateTime nowUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId required.", nameof(userId));
        ArgumentNullException.ThrowIfNull(device);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Device = device,
            LastSeenAtUtc = nowUtc
        };

        var family = RefreshTokenFamily.Create(userId, session.Id, nowUtc);
        session._family = family;

        session.RaiseDomainEvent(new UserSessionCreatedDomainEvent(
            userId, session.Id, device.DeviceId));

        return session;
    }

    /// <summary>
    /// Updates <see cref="LastSeenAtUtc"/>. No-op if the session is revoked.
    /// </summary>
    public void MarkSeen(DateTime nowUtc)
    {
        if (RevokedAtUtc is not null) return;
        LastSeenAtUtc = nowUtc;
    }

    /// <summary>
    /// Revokes this session. The bound refresh-token family is also revoked.
    /// Raises both <c>UserSessionRevokedDomainEvent</c> (for any revocation
    /// cause) and <c>UserLoggedOutDomainEvent</c> (user-initiated logout).
    /// Idempotent: calling twice does not raise a second event.
    /// </summary>
    public void Revoke(DateTime nowUtc, Guid? revokedByUserId, string reason)
    {
        if (RevokedAtUtc is not null) return;
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revoke reason is required.", nameof(reason));

        RevokedAtUtc = nowUtc;
        RevokedReason = reason;
        _family?.Revoke(nowUtc, reason);

        RaiseDomainEvent(new UserSessionRevokedDomainEvent(
            UserId, Id, revokedByUserId, reason));
        RaiseDomainEvent(new UserLoggedOutDomainEvent(UserId, Id));
    }

    /// <summary>
    /// Rotates the supplied old refresh token: marks it as consumed (with
    /// the new token id as the replacement), issues a new refresh token, adds
    /// it to the family, and raises <c>RefreshTokenRotatedDomainEvent</c>.
    /// Throws if the session is revoked.
    /// </summary>
    public RefreshToken RotateRefreshToken(
        RefreshToken oldToken,
        RefreshTokenHash newTokenHash,
        DateTime newExpiresAtUtc,
        string? createdByIp,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(oldToken);
        ArgumentNullException.ThrowIfNull(newTokenHash);

        if (RevokedAtUtc is not null)
            throw new InvalidOperationException("Cannot rotate refresh token on a revoked session.");

        // Issue the new token first (gets a new Id), then consume the old one
        // with that Id as the replacement target.
        var newToken = RefreshToken.Issue(
            UserId, _family!.Id, newTokenHash.Hex, newExpiresAtUtc, createdByIp);
        oldToken.Consume(newToken.Id, nowUtc);
        _family.AddToken(newToken);

        RaiseDomainEvent(new RefreshTokenRotatedDomainEvent(
            UserId, oldToken.Id, newToken.Id));

        return newToken;
    }
}
