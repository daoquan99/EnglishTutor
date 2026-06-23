using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Infrastructure.Audit;

internal sealed class UserSessionRevokedAuditHandler
    : IDomainEventHandler<UserSessionRevokedDomainEvent>
{
    private readonly ISecurityEventRecorder _recorder;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public UserSessionRevokedAuditHandler(
        ISecurityEventRecorder recorder,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _recorder = recorder;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(
        UserSessionRevokedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        string categoryCode;
        string sourceEventType;

        if (domainEvent.Reason == "user_logout")
        {
            categoryCode = AuditCategoryCodes.IdentityLogoutSucceeded;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentityLogoutSucceeded;
        }
        else if (domainEvent.Reason == "logout_all")
        {
            categoryCode = AuditCategoryCodes.IdentityLogoutAllSucceeded;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentityLogoutAllSucceeded;
        }
        else
        {
            categoryCode = AuditCategoryCodes.IdentitySessionRevoked;
            sourceEventType = AuditCategoryCodes.SourceEventTypes.IdentitySessionRevoked;
        }

        var request = new RecordSecurityEventRequest(
            CategoryCode: categoryCode,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
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
            OccurredAtUtc: domainEvent.OccurredAtUtc);

        await _recorder.RecordSecurityEventAsync(request, cancellationToken);
    }
}
