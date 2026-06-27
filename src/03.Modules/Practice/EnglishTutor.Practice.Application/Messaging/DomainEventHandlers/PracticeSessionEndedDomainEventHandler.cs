using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Contracts.Events;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;

namespace EnglishTutor.Practice.Application.Messaging.DomainEventHandlers;

public sealed class PracticeSessionEndedDomainEventHandler
    : IDomainEventHandler<PracticeSessionEndedDomainEvent>
{
    private readonly IPracticeIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public PracticeSessionEndedDomainEventHandler(
        IPracticeIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(
        PracticeSessionEndedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new PracticeSessionEndedIntegrationEventV1(
            SessionId: domainEvent.SessionId,
            UserId: domainEvent.UserId,
            DurationSeconds: domainEvent.DurationSeconds,
            EndReason: domainEvent.EndReason)
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId ?? domainEvent.EventId,
            SchemaVersion = "1"
        };

        return _publisher.StageAsync(
            integrationEvent: integrationEvent,
            cancellationToken: cancellationToken);
    }
}
