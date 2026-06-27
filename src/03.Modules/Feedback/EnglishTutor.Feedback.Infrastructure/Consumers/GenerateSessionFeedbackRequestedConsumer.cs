using System.Threading.Tasks;
using EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;
using EnglishTutor.Feedback.Contracts.Events;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class GenerateSessionFeedbackRequestedConsumer : IConsumer<GenerateSessionFeedbackRequestedV1>
{
    private readonly ISender _sender;
    private readonly ILogger<GenerateSessionFeedbackRequestedConsumer> _logger;

    public GenerateSessionFeedbackRequestedConsumer(ISender sender, ILogger<GenerateSessionFeedbackRequestedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateSessionFeedbackRequestedV1> context)
    {
        var ev = context.Message;
        _logger.LogInformation("Consuming GenerateSessionFeedbackRequested for session {SessionId}.", ev.SessionId);

        var result = await _sender.Send(new GenerateSessionFeedbackCommand(ev.SessionId, ev.UserId));
        if (result.IsFailure)
        {
            _logger.LogWarning("GenerateSessionFeedbackCommand failed for session {SessionId}. Error: {Error}", ev.SessionId, result.Error);
        }
    }
}
