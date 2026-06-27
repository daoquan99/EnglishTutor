using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;

namespace EnglishTutor.Learning.Application.Messaging.DomainEventHandlers;

public sealed class ModeDefinitionCreatedDomainEventHandler : IDomainEventHandler<ModeDefinitionCreatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ModeDefinitionCreatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ModeDefinitionCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ModeDefinitionCreatedIntegrationEvent(
            ModeDefinitionId: domainEvent.ModeDefinitionId,
            Code: domainEvent.Code,
            Name: domainEvent.Name)
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId ?? domainEvent.EventId,
            SchemaVersion = "1"
        };

        return _publisher.StageAsync(integrationEvent, cancellationToken);
    }
}

public sealed class ModeDefinitionUpdatedDomainEventHandler : IDomainEventHandler<ModeDefinitionUpdatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ModeDefinitionUpdatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ModeDefinitionUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ModeDefinitionUpdatedIntegrationEvent(
            ModeDefinitionId: domainEvent.ModeDefinitionId,
            Code: domainEvent.Code,
            Name: domainEvent.Name)
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId ?? domainEvent.EventId,
            SchemaVersion = "1"
        };

        return _publisher.StageAsync(integrationEvent, cancellationToken);
    }
}

public sealed class ModeDefinitionDisabledDomainEventHandler : IDomainEventHandler<ModeDefinitionDisabledDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ModeDefinitionDisabledDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ModeDefinitionDisabledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ModeDefinitionDisabledIntegrationEvent(domainEvent.ModeDefinitionId)
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId ?? domainEvent.EventId,
            SchemaVersion = "1"
        };

        return _publisher.StageAsync(integrationEvent, cancellationToken);
    }
}
