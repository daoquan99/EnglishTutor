using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Session created on successful login -> durable LoginSucceeded security event
// (Batch R1, H-07). Staged in the Identity outbox, consumed idempotently by Audit.
internal sealed class UserSessionCreatedAuditHandler
    : IDomainEventHandler<UserSessionCreatedDomainEvent>
{
    private readonly IIdentitySecurityEventPublisher _publisher;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public UserSessionCreatedAuditHandler(
        IIdentitySecurityEventPublisher publisher,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _publisher = publisher;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(
        UserSessionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: IdentitySecurityEventTypes.LoginSucceeded,
            CategoryCode: AuditCategoryCodes.IdentityLoginSucceeded,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityLoginSucceeded,
            UserId: domainEvent.UserId,
            SessionId: domainEvent.SessionId,
            RefreshTokenFamilyId: null,
            RefreshTokenId: null,
            ReasonCode: null,
            IpAddressHash: _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: domainEvent.OccurredAtUtc),
            cancellationToken);
    }
}
