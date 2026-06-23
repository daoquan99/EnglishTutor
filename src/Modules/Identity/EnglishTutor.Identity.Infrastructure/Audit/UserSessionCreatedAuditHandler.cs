using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Infrastructure.Audit;

internal sealed class UserSessionCreatedAuditHandler
    : IDomainEventHandler<UserSessionCreatedDomainEvent>
{
    private readonly ISecurityEventRecorder _recorder;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public UserSessionCreatedAuditHandler(
        ISecurityEventRecorder recorder,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _recorder = recorder;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(
        UserSessionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var request = new RecordSecurityEventRequest(
            CategoryCode: AuditCategoryCodes.IdentityLoginSucceeded,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
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
            OccurredAtUtc: domainEvent.OccurredAtUtc);

        await _recorder.RecordSecurityEventAsync(request, cancellationToken);
    }
}
