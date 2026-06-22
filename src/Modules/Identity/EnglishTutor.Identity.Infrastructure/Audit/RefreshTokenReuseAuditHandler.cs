using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Identity.Infrastructure handler that bridges
// RefreshTokenReuseDetectedDomainEvent to EnglishTutor.Audit.Contracts.ISecurityEventRecorder.
//
// This handler lives in Identity.Infrastructure (NOT Audit.Infrastructure) so
// that Audit.Infrastructure does NOT need to reference Identity.Domain.
// Identity.Infrastructure references Audit.Contracts (the only Audit
// assembly Identity may depend on).
internal sealed class RefreshTokenReuseAuditHandler
    : IDomainEventHandler<RefreshTokenReuseDetectedDomainEvent>
{
    private readonly ISecurityEventRecorder _recorder;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;
    private readonly ILogger<RefreshTokenReuseAuditHandler> _logger;

    public RefreshTokenReuseAuditHandler(
        ISecurityEventRecorder recorder,
        IRefreshTokenReuseContextAccessor contextAccessor,
        ILogger<RefreshTokenReuseAuditHandler> logger)
    {
        _recorder = recorder;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public async Task HandleAsync(
        RefreshTokenReuseDetectedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // SECURITY: do not log the raw refresh token (it is not in scope of
        // this event anyway). Log only the ids + the category code.
        _logger.LogWarning(
            "RefreshTokenReuseDetected recorded for audit. Category={Category} UserId={UserId} FamilyId={FamilyId} TokenId={TokenId} SessionId={SessionId}.",
            AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            domainEvent.UserId,
            domainEvent.RefreshTokenFamilyId,
            domainEvent.RefreshTokenId,
            domainEvent.SessionId);

        var request = new RecordRefreshTokenReuseRequest(
            UserId: domainEvent.UserId,
            SessionId: domainEvent.SessionId,
            RefreshTokenFamilyId: domainEvent.RefreshTokenFamilyId,
            RefreshTokenId: domainEvent.RefreshTokenId,
            ReasonCode: domainEvent.Reason,
            IpAddressHash: _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: domainEvent.OccurredAtUtc);

        await _recorder.RecordRefreshTokenReuseAsync(request, cancellationToken);
    }
}
