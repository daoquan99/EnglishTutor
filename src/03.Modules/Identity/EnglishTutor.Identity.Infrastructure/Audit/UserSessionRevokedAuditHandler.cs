using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Session revocation / logout / logout-all -> durable security event
// (Batch R1, H-07). Staged in the Identity outbox, consumed idempotently by Audit.
internal sealed class UserSessionRevokedAuditHandler
    : IDomainEventHandler<UserSessionRevokedDomainEvent>
{
    private readonly IIdentitySecurityEventPublisher _publisher;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public UserSessionRevokedAuditHandler(
        IIdentitySecurityEventPublisher publisher,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _publisher = publisher;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(
        UserSessionRevokedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        string categoryCode;
        string sourceEventType;
        string eventType;

        if (domainEvent.Reason == "user_logout")
        {
            categoryCode = AuditCategoryCodes.IdentityLogoutSucceeded;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentityLogoutSucceeded;
            eventType = IdentitySecurityEventTypes.LoggedOut;
        }
        else if (domainEvent.Reason == "logout_all")
        {
            categoryCode = AuditCategoryCodes.IdentityLogoutAllSucceeded;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentityLogoutAllSucceeded;
            eventType = IdentitySecurityEventTypes.LoggedOutAll;
        }
        else
        {
            categoryCode = AuditCategoryCodes.IdentitySessionRevoked;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentitySessionRevoked;
            eventType = IdentitySecurityEventTypes.SessionRevoked;
        }

        await _publisher.PublishAsync(new IdentitySecurityEventData(
            EventType: eventType,
            CategoryCode: categoryCode,
            SourceEventType: sourceEventType,
            UserId: domainEvent.UserId,
            SessionId: domainEvent.SessionId,
            RefreshTokenFamilyId: null,
            RefreshTokenId: null,
            ReasonCode: domainEvent.Reason,
            IpAddressHash: _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: domainEvent.OccurredAtUtc),
            cancellationToken);
    }
}
