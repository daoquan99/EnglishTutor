using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

namespace EnglishTutor.Learning.Application.Messaging.DomainEventHandlers;

public sealed class TopicCreatedDomainEventHandler : IDomainEventHandler<TopicCreatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public TopicCreatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(TopicCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new TopicCreatedIntegrationEvent(
            TopicId: domainEvent.TopicId,
            Name: domainEvent.Name,
            Slug: domainEvent.Slug)
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

public sealed class TopicUpdatedDomainEventHandler : IDomainEventHandler<TopicUpdatedDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public TopicUpdatedDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(TopicUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new TopicUpdatedIntegrationEvent(
            TopicId: domainEvent.TopicId,
            Name: domainEvent.Name,
            Slug: domainEvent.Slug,
            Description: domainEvent.Description)
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

public sealed class TopicDisabledDomainEventHandler : IDomainEventHandler<TopicDisabledDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public TopicDisabledDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(TopicDisabledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new TopicDisabledIntegrationEvent(domainEvent.TopicId)
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

public sealed class TopicModeEnabledDomainEventHandler : IDomainEventHandler<TopicModeEnabledDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public TopicModeEnabledDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(TopicModeEnabledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new TopicModeEnabledIntegrationEvent(
            TopicId: domainEvent.TopicId,
            ModeDefinitionId: domainEvent.ModeDefinitionId,
            ConfigJson: domainEvent.ConfigJson)
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

public sealed class TopicModeDisabledDomainEventHandler : IDomainEventHandler<TopicModeDisabledDomainEvent>
{
    private readonly ILearningIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public TopicModeDisabledDomainEventHandler(
        ILearningIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(TopicModeDisabledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new TopicModeDisabledIntegrationEvent(
            TopicId: domainEvent.TopicId,
            ModeDefinitionId: domainEvent.ModeDefinitionId)
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
