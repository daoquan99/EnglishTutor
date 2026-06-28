using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Infrastructure.Persistence;

namespace EnglishTutor.Identity.Infrastructure.Audit;

internal sealed class NativeIdentitySecurityEventPublisher : IIdentitySecurityEventPublisher
{
    private readonly NativeOutboxWriter<IdentityDbContext> _outbox;

    public NativeIdentitySecurityEventPublisher(NativeOutboxWriter<IdentityDbContext> outbox)
    {
        _outbox = outbox;
    }

    public Task PublishAsync(
        IdentitySecurityEventData data,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new IdentitySecurityEventRecordedV1
        {
            EventType = data.EventType,
            CategoryCode = data.CategoryCode,
            SourceModule = AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType = data.SourceEventType,
            UserId = data.UserId,
            SessionId = data.SessionId,
            RefreshTokenFamilyId = data.RefreshTokenFamilyId,
            RefreshTokenId = data.RefreshTokenId,
            ReasonCode = data.ReasonCode,
            IpAddressHash = data.IpAddressHash,
            UserAgentHash = data.UserAgentHash,
            OccurredAtUtc = data.OccurredAtUtc,
            CorrelationId = data.CorrelationId,
            CausationId = data.CausationId
        };

        return _outbox.StageAsync(integrationEvent, cancellationToken);
    }
}
