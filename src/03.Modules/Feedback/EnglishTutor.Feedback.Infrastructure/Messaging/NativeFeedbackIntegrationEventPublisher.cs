using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.Feedback.Infrastructure.Persistence;

namespace EnglishTutor.Feedback.Infrastructure.Messaging;

internal sealed class NativeFeedbackIntegrationEventPublisher : IFeedbackIntegrationEventPublisher
{
    private readonly NativeOutboxWriter<FeedbackDbContext> _outbox;

    public NativeFeedbackIntegrationEventPublisher(NativeOutboxWriter<FeedbackDbContext> outbox)
    {
        _outbox = outbox;
    }

    public Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        _outbox.StageAsync(integrationEvent, cancellationToken);
}
