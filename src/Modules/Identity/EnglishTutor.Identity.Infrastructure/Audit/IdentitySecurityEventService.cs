using EnglishTutor.Audit.Contracts;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Tracks login-failed and refresh-failed/rejected security events DURABLY
// (Batch R1, H-07). Each call stages an IdentitySecurityEventRecordedV1 into
// the Identity transactional outbox via the publisher; the event is committed
// atomically with the Identity state change on the handler's SaveChanges, and
// the Audit module consumes it idempotently.
//
// SECURITY: only hashed IP/UA, ids, and stable reason codes are emitted —
// never raw tokens/hashes/passwords/cookies/raw IP/raw UA/headers.
internal sealed class IdentitySecurityEventService : IIdentitySecurityEventService
{
    private readonly IIdentitySecurityEventPublisher _publisher;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public IdentitySecurityEventService(
        IIdentitySecurityEventPublisher publisher,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _publisher = publisher;
        _contextAccessor = contextAccessor;
    }

    public async Task TrackLoginFailedAsync(
        Guid? userId,
        string reasonCode,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: IdentitySecurityEventTypes.LoginFailed,
            CategoryCode: AuditCategoryCodes.IdentityLoginFailed,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityLoginFailed,
            UserId: userId,
            SessionId: null,
            RefreshTokenFamilyId: null,
            RefreshTokenId: null,
            ReasonCode: reasonCode,
            IpAddressHash: Hash(ipAddress) ?? _contextAccessor.IpAddressHash,
            UserAgentHash: Hash(userAgent) ?? _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: DateTime.UtcNow),
            cancellationToken);
    }

    public async Task TrackRefreshFailedAsync(
        Guid? sessionId,
        Guid? userId,
        Guid? refreshTokenFamilyId,
        Guid? refreshTokenId,
        string reasonCode,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: IdentitySecurityEventTypes.RefreshRejected,
            CategoryCode: AuditCategoryCodes.IdentityRefreshFailed,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshFailed,
            UserId: userId,
            SessionId: sessionId,
            RefreshTokenFamilyId: refreshTokenFamilyId,
            RefreshTokenId: refreshTokenId,
            ReasonCode: reasonCode,
            IpAddressHash: Hash(ipAddress) ?? _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: DateTime.UtcNow),
            cancellationToken);
    }

    private static string? Hash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
