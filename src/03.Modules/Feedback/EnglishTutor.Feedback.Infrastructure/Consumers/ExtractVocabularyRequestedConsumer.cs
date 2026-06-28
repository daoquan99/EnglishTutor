using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class ExtractVocabularyRequestedConsumer
    : RabbitMqMessageHandler<ExtractVocabularyRequestedV1>
{
    public const string ConsumerName = "feedback.extract-vocabulary.v1";
    public const string QueueName = "english.feedback.extract-vocabulary.v1";

    private readonly FeedbackDbContext _dbContext;
    private readonly ILogger<ExtractVocabularyRequestedConsumer> _logger;

    public ExtractVocabularyRequestedConsumer(
        FeedbackDbContext dbContext,
        ILogger<ExtractVocabularyRequestedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName,
        QueueName,
        typeof(ExtractVocabularyRequestedV1));

    protected override async Task HandleAsync(
        ExtractVocabularyRequestedV1 message,
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
            "Vocabulary extraction is already included for session {SessionId}.",
            message.SessionId);
        _dbContext.InboxMessages.Add(new InboxMessage(
            context.MessageId,
            ConsumerName,
            context.ContractName,
            context.SchemaVersion,
            context.ReceivedAtUtc));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
