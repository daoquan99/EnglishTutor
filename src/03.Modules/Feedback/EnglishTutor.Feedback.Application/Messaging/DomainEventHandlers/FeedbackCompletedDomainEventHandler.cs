using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Events;

namespace EnglishTutor.Feedback.Application.Messaging.DomainEventHandlers;

public sealed class FeedbackCompletedDomainEventHandler
    : IDomainEventHandler<FeedbackCompletedDomainEvent>
{
    private readonly IFeedbackIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public FeedbackCompletedDomainEventHandler(
        IFeedbackIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _eventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));
    }

    public Task HandleAsync(
        FeedbackCompletedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;

        var integrationEvent = new FeedbackReadyIntegrationEventV1(
            SessionId: domainEvent.SessionId,
            UserId: domainEvent.UserId)
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
