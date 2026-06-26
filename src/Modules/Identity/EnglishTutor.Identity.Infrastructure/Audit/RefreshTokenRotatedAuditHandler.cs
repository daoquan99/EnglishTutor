using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Refresh-token rotation success -> durable RefreshSucceeded security event
// (Batch R1, H-07). Staged in the Identity outbox, consumed idempotently by Audit.
internal sealed class RefreshTokenRotatedAuditHandler
    : IDomainEventHandler<RefreshTokenRotatedDomainEvent>
{
    private readonly IIdentitySecurityEventPublisher _publisher;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public RefreshTokenRotatedAuditHandler(
        IIdentitySecurityEventPublisher publisher,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _publisher = publisher;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(
        RefreshTokenRotatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: IdentitySecurityEventTypes.RefreshSucceeded,
            CategoryCode: AuditCategoryCodes.IdentityRefreshSucceeded,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshSucceeded,
            UserId: domainEvent.UserId,
            SessionId: domainEvent.SessionId,
            RefreshTokenFamilyId: null,
            RefreshTokenId: domainEvent.NewTokenId,
            ReasonCode: null,
            IpAddressHash: _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: domainEvent.OccurredAtUtc),
            cancellationToken);
    }
}
