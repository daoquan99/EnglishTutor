using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Events;

namespace EnglishTutor.Feedback.Application.Messaging.DomainEventHandlers;

public sealed class FeedbackCompletedV2DomainEventHandler
    : IDomainEventHandler<FeedbackCompletedDomainEvent>
{
    private readonly IFeedbackIntegrationEventPublisher _publisher;
    private readonly IEventEnvelopeContextAccessor _eventContext;

    public FeedbackCompletedV2DomainEventHandler(
        IFeedbackIntegrationEventPublisher publisher,
        IEventEnvelopeContextAccessor eventContext)
    {
        _publisher = publisher;
        _eventContext = eventContext;
    }

    public Task HandleAsync(
        FeedbackCompletedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var context = _eventContext.Current;
        var integrationEvent = new FeedbackReadyIntegrationEventV2(
            FeedbackId: domainEvent.FeedbackId,
            SessionId: domainEvent.SessionId,
            UserId: domainEvent.UserId,
            LanguagePairId: domainEvent.LanguagePairId,
            NativeLanguageCode: domainEvent.NativeLanguageCode,
            TargetLanguageCode: domainEvent.TargetLanguageCode,
            Score: domainEvent.Score,
            CefrLevel: domainEvent.CefrLevel)
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId ?? domainEvent.EventId,
            SchemaVersion = "2"
        };

        return _publisher.StageAsync(integrationEvent, cancellationToken);
    }
}
