using EnglishTutor.Audit.Contracts;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Contracts.Events;
using EnglishTutor.Identity.Infrastructure.Messaging;
using MassTransit;
using MassTransit.DependencyInjection;

namespace EnglishTutor.Identity.Infrastructure.Audit;

internal sealed class OutboxIdentitySecurityEventPublisher : IIdentitySecurityEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OutboxIdentitySecurityEventPublisher(Bind<IIdentityBus, IPublishEndpoint> publishEndpoint)
    {
        _publishEndpoint = publishEndpoint.Value;
    }

    public Task PublishAsync(IdentitySecurityEventData data, CancellationToken cancellationToken = default)
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
            CausationId = data.CausationId,
        };

        return _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
