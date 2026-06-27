using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Bridges RefreshTokenReuseDetectedDomainEvent to a DURABLE security
// integration event (Batch R1, H-07). The event is staged in the Identity
// transactional outbox and committed atomically with the Identity state change;
// the Audit module consumes it idempotently. Replaces the prior best-effort
// in-process ISecurityEventRecorder call.
internal sealed class RefreshTokenReuseAuditHandler
    : IDomainEventHandler<RefreshTokenReuseDetectedDomainEvent>
{
    private readonly IIdentitySecurityEventPublisher _publisher;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;
    private readonly ILogger<RefreshTokenReuseAuditHandler> _logger;

    public RefreshTokenReuseAuditHandler(
        IIdentitySecurityEventPublisher publisher,
        IRefreshTokenReuseContextAccessor contextAccessor,
        ILogger<RefreshTokenReuseAuditHandler> logger)
    {
        _publisher = publisher;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public async Task HandleAsync(
        RefreshTokenReuseDetectedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // SECURITY: ids + category only — never the raw refresh token.
        _logger.LogWarning(
            "RefreshTokenReuseDetected staged for durable audit. Category={Category} UserId={UserId} FamilyId={FamilyId} TokenId={TokenId} SessionId={SessionId}.",
            AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            domainEvent.UserId,
            domainEvent.RefreshTokenFamilyId,
            domainEvent.RefreshTokenId,
            domainEvent.SessionId);

        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: IdentitySecurityEventTypes.RefreshTokenReuseDetected,
            CategoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            UserId: domainEvent.UserId,
            SessionId: domainEvent.SessionId,
            RefreshTokenFamilyId: domainEvent.RefreshTokenFamilyId,
            RefreshTokenId: domainEvent.RefreshTokenId,
            ReasonCode: domainEvent.Reason,
            IpAddressHash: _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: domainEvent.OccurredAtUtc),
            cancellationToken);
    }
}
