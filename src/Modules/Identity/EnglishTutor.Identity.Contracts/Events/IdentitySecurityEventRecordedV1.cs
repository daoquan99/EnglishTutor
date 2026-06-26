using System;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Identity.Contracts.Events;

/// <summary>
/// Durable integration event published by the Identity module whenever a
/// security-relevant fact occurs (Batch R1, H-07). It is written to the
/// Identity transactional outbox <b>atomically</b> with the Identity state
/// change that produced it, then delivered to the Audit module which records
/// it idempotently.
/// </summary>
/// <remarks>
/// <para>
/// This is a single versioned <b>carrier</b> contract rather than eight
/// near-identical event types. The concrete security fact is identified by
/// <see cref="EventType"/> (a stable code from
/// <see cref="IdentitySecurityEventTypes"/>) and <see cref="CategoryCode"/>,
/// which keeps the Audit consumer a single idempotent mapping and matches the
/// existing <c>RecordSecurityEvent</c> shape one-to-one. The candidate events
/// from the Phase 1 design (LoginSucceeded/LoginFailed/RefreshSucceeded/
/// RefreshTokenReuseDetected/RefreshRejected/SessionRevoked/LoggedOut/
/// LoggedOutAll) are represented by <see cref="EventType"/> values.
/// </para>
/// <para>
/// <b>Privacy (security-identity.md / privacy-retention.md):</b> the payload
/// carries only safe identifiers, stable reason/category codes, and already
/// hashed IP / user-agent values. It MUST NEVER carry a raw refresh token, a
/// refresh-token hash, a password, a cookie value, a raw IP, a raw user-agent,
/// a raw header, a stack trace, or any other secret.
/// </para>
/// <para>
/// <see cref="IntegrationEvent"/> supplies <c>EventId</c>, <c>OccurredAtUtc</c>,
/// <c>SchemaVersion</c>, <c>CorrelationId</c>, and <c>CausationId</c>.
/// <c>OccurredAtUtc</c> is set to the time the underlying security fact
/// occurred.
/// </para>
/// </remarks>
public sealed record IdentitySecurityEventRecordedV1 : IntegrationEvent
{
    /// <summary>Stable security-event discriminator (see <see cref="IdentitySecurityEventTypes"/>).</summary>
    public required string EventType { get; init; }

    /// <summary>Audit category code (e.g. <c>identity.refresh_token_reuse_detected</c>).</summary>
    public required string CategoryCode { get; init; }

    /// <summary>Owning source module code (always <c>identity</c>).</summary>
    public required string SourceModule { get; init; }

    /// <summary>Fully-qualified source event type string for the audit row.</summary>
    public required string SourceEventType { get; init; }

    public Guid? UserId { get; init; }
    public Guid? SessionId { get; init; }
    public Guid? RefreshTokenFamilyId { get; init; }
    public Guid? RefreshTokenId { get; init; }

    /// <summary>Stable, safe reason code (never free-form secret text).</summary>
    public string? ReasonCode { get; init; }

    /// <summary>SHA-256 hash of the client IP (never the raw IP).</summary>
    public string? IpAddressHash { get; init; }

    /// <summary>SHA-256 hash of the user-agent (never the raw UA).</summary>
    public string? UserAgentHash { get; init; }
}
