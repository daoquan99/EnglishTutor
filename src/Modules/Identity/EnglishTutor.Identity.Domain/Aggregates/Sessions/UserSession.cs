using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions;

// Aggregate root for the Sessions aggregate. Represents one server-side
// login session: device binding, refresh-token family, last-seen, and
// revocation state. Soft-delete and audit metadata come from
// AggregateRoot.
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

    // Factory: creates a new UserSession and a bound
    // RefreshTokenFamily. Raises UserSessionCreatedDomainEvent.
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

    // Updates LastSeenAtUtc. No-op if the session is revoked.
    public void MarkSeen(DateTime nowUtc)
    {
        if (RevokedAtUtc is not null) return;
        LastSeenAtUtc = nowUtc;
    }

    // Revokes this session. The bound refresh-token family is also revoked.
    // Raises both UserSessionRevokedDomainEvent (for any revocation
    // cause) and UserLoggedOutDomainEvent (user-initiated logout).
    // Idempotent: calling twice does not raise a second event.
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

    // Detects refresh-token reuse: marks the supplied token as
    // reuse-detected (raises RefreshTokenReuseDetectedDomainEvent with
    // this session id as runtime context), and revokes the entire
    // session + family. This is the new entry point used by
    // RefreshCommandHandler for the reuse path; the handler does NOT
    // call RefreshToken.MarkReuseDetected directly any more, so the
    // SessionId is available at the aggregate root and can be passed
    // through to the event without persisting it on RefreshToken.
    public void DetectRefreshTokenReuse(
        RefreshToken token,
        DateTime nowUtc,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        // Token must belong to this session's family. Otherwise we have
        // a programming error (caller passed the wrong token).
        if (_family is null || token.FamilyId != _family.Id)
        {
            throw new InvalidOperationException(
                "Refresh token does not belong to this session's family.");
        }

        // Mark the token as reuse-detected. Pass the session id as
        // runtime context so the raised event carries it without
        // persisting it on the token.
        token.MarkReuseDetected(nowUtc, reason, sessionId: Id);

        // Revoke the session. The family revocation cascades inside
        // Revoke -> _family.Revoke -> each token.Revoke.
        Revoke(nowUtc, revokedByUserId: null, reason: reason);
    }

    // Rotates the supplied old refresh token: marks it as consumed (with
    // the new token id as the replacement), issues a new refresh token, adds
    // it to the family, and raises RefreshTokenRotatedDomainEvent.
    // Throws if the session is revoked.
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
            UserId, Id, oldToken.Id, newToken.Id));

        return newToken;
    }
}
