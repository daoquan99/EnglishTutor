using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;

namespace EnglishTutor.Learning.Application.Messaging.DomainEventHandlers;

public sealed class ScenarioCreatedDomainEventHandler : IDomainEventHandler<ScenarioCreatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ScenarioCreatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ScenarioCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ScenarioCreatedIntegrationEvent(
            ScenarioId: domainEvent.ScenarioId,
            TopicId: domainEvent.TopicId,
            ModeDefinitionId: domainEvent.ModeDefinitionId,
            Name: domainEvent.Name,
            DifficultyLevel: domainEvent.DifficultyLevel)
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

public sealed class ScenarioUpdatedDomainEventHandler : IDomainEventHandler<ScenarioUpdatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ScenarioUpdatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ScenarioUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ScenarioUpdatedIntegrationEvent(
            ScenarioId: domainEvent.ScenarioId,
            TopicId: domainEvent.TopicId,
            ModeDefinitionId: domainEvent.ModeDefinitionId,
            Name: domainEvent.Name,
            DifficultyLevel: domainEvent.DifficultyLevel,
            PromptTemplate: domainEvent.PromptTemplate)
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

public sealed class ScenarioDisabledDomainEventHandler : IDomainEventHandler<ScenarioDisabledDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public ScenarioDisabledDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(ScenarioDisabledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new ScenarioDisabledIntegrationEvent(domainEvent.ScenarioId)
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
