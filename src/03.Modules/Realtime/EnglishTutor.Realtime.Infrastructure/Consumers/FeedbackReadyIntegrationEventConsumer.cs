using System;
using System.Threading.Tasks;
using EnglishTutor.Feedback.Contracts;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Realtime.Application.Abstractions;
using MassTransit;

namespace EnglishTutor.Realtime.Infrastructure.Consumers;

public sealed class FeedbackReadyIntegrationEventConsumer : IConsumer<FeedbackReadyIntegrationEventV1>
{
    private readonly IFeedbackModule _feedbackModule;
    private readonly IRealtimeNotifier _notifier;

    public FeedbackReadyIntegrationEventConsumer(
        IFeedbackModule feedbackModule,
        IRealtimeNotifier notifier)
    {
        _feedbackModule = feedbackModule ?? throw new ArgumentNullException(nameof(feedbackModule));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public async Task Consume(ConsumeContext<FeedbackReadyIntegrationEventV1> context)
    {
        var message = context.Message;
        
        // Retrieve the feedback using the cross-module Contract
        var result = await _feedbackModule.GetSessionFeedbackAsync(message.UserId, message.SessionId, context.CancellationToken);
        if (result != null && result.Status == "Success")
        {
            await _notifier.NotifyFeedbackReadyAsync(
                message.SessionId,
                message.CorrelationId,
                result.Score?.ToString() ?? "0",
                result.Summary ?? string.Empty,
                context.CancellationToken);
        }
    }
}
