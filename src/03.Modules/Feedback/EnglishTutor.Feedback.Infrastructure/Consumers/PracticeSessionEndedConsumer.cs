using System;
using System.Threading.Tasks;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Practice.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class PracticeSessionEndedConsumer : IConsumer<PracticeSessionEndedIntegrationEventV1>
{
    private readonly ILogger<PracticeSessionEndedConsumer> _logger;

    public PracticeSessionEndedConsumer(ILogger<PracticeSessionEndedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PracticeSessionEndedIntegrationEventV1> context)
    {
        var ev = context.Message;
        _logger.LogInformation("Processing PracticeSessionEnded for session {SessionId}.", ev.SessionId);

        // Publish the feedback generation request event (feedback job request)
        await context.Publish(new GenerateSessionFeedbackRequestedV1(ev.SessionId, ev.UserId));
    }
}
