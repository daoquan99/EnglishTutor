using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Practice.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class PracticeSessionEndedConsumer
    : RabbitMqMessageHandler<PracticeSessionEndedIntegrationEventV1>
{
    public const string ConsumerName = "feedback.practice-session-ended.v1";
    public const string QueueName = "english.feedback.practice-session-ended.v1";

    private readonly ILogger<PracticeSessionEndedConsumer> _logger;
    private readonly FeedbackDbContext _dbContext;
    private readonly NativeOutboxWriter<FeedbackDbContext> _outbox;

    public PracticeSessionEndedConsumer(
        ILogger<PracticeSessionEndedConsumer> logger,
        FeedbackDbContext dbContext,
        NativeOutboxWriter<FeedbackDbContext> outbox)
    {
        _logger = logger;
        _dbContext = dbContext;
        _outbox = outbox;
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName,
        QueueName,
        typeof(PracticeSessionEndedIntegrationEventV1));

    protected override async Task HandleAsync(
        PracticeSessionEndedIntegrationEventV1 message,
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
            "Processing PracticeSessionEnded for session {SessionId}.",
            message.SessionId);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.InboxMessages.Add(new InboxMessage(
            context.MessageId,
            ConsumerName,
            context.ContractName,
            context.SchemaVersion,
            context.ReceivedAtUtc));
        await _outbox.StageAsync(
            new GenerateSessionFeedbackRequestedV1(message.SessionId, message.UserId)
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId
            },
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
