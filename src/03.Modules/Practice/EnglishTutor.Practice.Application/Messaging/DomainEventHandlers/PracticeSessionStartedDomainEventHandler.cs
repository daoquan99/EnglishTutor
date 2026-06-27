using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Contracts.Events;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;

namespace EnglishTutor.Practice.Application.Messaging.DomainEventHandlers;

public sealed class PracticeSessionStartedDomainEventHandler
    : IDomainEventHandler<PracticeSessionStartedDomainEvent>
{
    private readonly IPracticeIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public PracticeSessionStartedDomainEventHandler(
        IPracticeIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(
        PracticeSessionStartedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new PracticeSessionStartedIntegrationEventV1(
            SessionId: domainEvent.SessionId,
            UserId: domainEvent.UserId,
            ScenarioId: domainEvent.ScenarioId,
            ModeDefinitionId: domainEvent.ModeDefinitionId)
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
