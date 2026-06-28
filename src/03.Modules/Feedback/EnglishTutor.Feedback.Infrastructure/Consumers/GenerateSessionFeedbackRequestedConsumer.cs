using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class GenerateSessionFeedbackRequestedConsumer
    : RabbitMqMessageHandler<GenerateSessionFeedbackRequestedV1>
{
    public const string ConsumerName = "feedback.generate-session.v1";
    public const string QueueName = "english.feedback.generate-session.v1";

    private readonly ISender _sender;
    private readonly FeedbackDbContext _dbContext;
    private readonly ILogger<GenerateSessionFeedbackRequestedConsumer> _logger;

    public GenerateSessionFeedbackRequestedConsumer(
        ISender sender,
        FeedbackDbContext dbContext,
        ILogger<GenerateSessionFeedbackRequestedConsumer> logger)
    {
        _sender = sender;
        _dbContext = dbContext;
        _logger = logger;
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName,
        QueueName,
        typeof(GenerateSessionFeedbackRequestedV1),
        PrefetchCount: 2,
        Concurrency: 2);

    protected override async Task HandleAsync(
        GenerateSessionFeedbackRequestedV1 message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.InboxMessages.AnyAsync(
                inbox => inbox.MessageId == context.MessageId &&
                         inbox.ConsumerName == ConsumerName,
                cancellationToken))
        {
            return;
        }

        _logger.LogInformation(
            "Consuming GenerateSessionFeedbackRequested for session {SessionId}.",
            message.SessionId);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.InboxMessages.Add(new InboxMessage(
            context.MessageId,
            ConsumerName,
            context.ContractName,
            context.SchemaVersion,
            context.ReceivedAtUtc));
        var result = await _sender.Send(
            new GenerateSessionFeedbackCommand(message.SessionId, message.UserId),
            cancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"GenerateSessionFeedback failed with code '{result.Error?.Code ?? "unknown"}'.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
