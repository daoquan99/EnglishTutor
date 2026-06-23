using EnglishTutor.BuildingBlocks.Domain.Aggregates;


namespace EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;

// Audit module aggregate root. One row per recorded security event. Stored
// in the audit schema. No FK to any other module's tables.
//
// The aggregate inherits audit + soft-delete columns from AggregateRoot so
// Audit module events themselves can be soft-deleted (e.g. for GDPR
// erasure) and audited (who recorded what, when).
public sealed class SecurityEvent : AggregateRoot
{
    public string CategoryCode { get; private set; } = string.Empty;
    public string SourceModule { get; private set; } = string.Empty;
    public string SourceEventType { get; private set; } = string.Empty;

    // Identity ids. Nullable so non-Identity events can also be recorded
    // in the future (audit module is module-agnostic).
    public Guid? UserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public Guid? RefreshTokenFamilyId { get; private set; }
    public Guid? RefreshTokenId { get; private set; }

    public string? ReasonCode { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? CausationId { get; private set; }

    // Hashed context. Audit.Infrastructure does NOT see the plaintext.
    public string? IpAddressHash { get; private set; }
    public string? UserAgentHash { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    private SecurityEvent() { }

    public static SecurityEvent Create(
        string categoryCode,
        string sourceModule,
        string sourceEventType,
        Guid? userId,
        Guid? sessionId,
        Guid? refreshTokenFamilyId,
        Guid? refreshTokenId,
        string? reasonCode,
        Guid? correlationId,
        Guid? causationId,
        string? ipAddressHash,
        string? userAgentHash,
        DateTime occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
            throw new ArgumentException("CategoryCode required.", nameof(categoryCode));
        if (string.IsNullOrWhiteSpace(sourceModule))
            throw new ArgumentException("SourceModule required.", nameof(sourceModule));
        if (string.IsNullOrWhiteSpace(sourceEventType))
            throw new ArgumentException("SourceEventType required.", nameof(sourceEventType));

        return new SecurityEvent
        {
            Id = Guid.NewGuid(),
            CategoryCode = categoryCode,
            SourceModule = sourceModule,
            SourceEventType = sourceEventType,
            UserId = userId,
            SessionId = sessionId,
            RefreshTokenFamilyId = refreshTokenFamilyId,
            RefreshTokenId = refreshTokenId,
            ReasonCode = reasonCode,
            CorrelationId = correlationId,
            CausationId = causationId,
            IpAddressHash = ipAddressHash,
            UserAgentHash = userAgentHash,
            OccurredAtUtc = occurredAtUtc
        };
    }
}
