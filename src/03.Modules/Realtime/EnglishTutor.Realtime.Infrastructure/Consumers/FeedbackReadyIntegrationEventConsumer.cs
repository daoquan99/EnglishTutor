using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Contracts;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Realtime.Application.Abstractions;

namespace EnglishTutor.Realtime.Infrastructure.Consumers;

public sealed class FeedbackReadyIntegrationEventConsumer
    : RabbitMqMessageHandler<FeedbackReadyIntegrationEventV1>
{
    public const string ConsumerName = "realtime.feedback-ready.v1";
    public const string QueueName = "english.realtime.feedback-ready.v1";

    private readonly IFeedbackModule _feedbackModule;
    private readonly IRealtimeNotifier _notifier;

    public FeedbackReadyIntegrationEventConsumer(
        IFeedbackModule feedbackModule,
        IRealtimeNotifier notifier)
    {
        _feedbackModule = feedbackModule ?? throw new ArgumentNullException(nameof(feedbackModule));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName,
        QueueName,
        typeof(FeedbackReadyIntegrationEventV1),
        PrefetchCount: 32,
        Concurrency: 4);

    protected override async Task HandleAsync(
        FeedbackReadyIntegrationEventV1 message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        var result = await _feedbackModule.GetSessionFeedbackAsync(
            message.UserId,
            message.SessionId,
            cancellationToken);
        if (result != null && result.Status == "Success")
        {
            await _notifier.NotifyFeedbackReadyAsync(
                message.SessionId,
                message.CorrelationId,
                result.Score?.ToString() ?? "0",
                result.Summary ?? string.Empty,
                cancellationToken);
        }
    }
}
