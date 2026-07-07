using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Practice.Contracts.Events;
using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress;
using EnglishTutor.Progress.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Progress.Infrastructure.Consumers;

public sealed class PracticeSessionEndedV2Consumer
    : RabbitMqMessageHandler<PracticeSessionEndedIntegrationEventV2>
{
    public const string ConsumerName = "progress.practice-session-ended.v2";
    public const string QueueName = "english.progress.practice-session-ended.v2";

    private readonly ProgressDbContext _dbContext;

    public PracticeSessionEndedV2Consumer(ProgressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName,
        QueueName,
        typeof(PracticeSessionEndedIntegrationEventV2));

    protected override async Task HandleAsync(
        PracticeSessionEndedIntegrationEventV2 message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.InboxMessages.AnyAsync(
                item => item.MessageId == context.MessageId &&
                        item.ConsumerName == ConsumerName,
                cancellationToken))
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var progress = await _dbContext.LearnerLanguageProgress.FirstOrDefaultAsync(
            item => item.UserId == message.UserId &&
                    item.LanguagePairId == message.LanguagePairId,
            cancellationToken);
        if (progress is null)
        {
            progress = LearnerLanguageProgress.Create(
                Guid.NewGuid(),
                message.UserId,
                message.LanguagePairId,
                message.NativeLanguageCode,
                message.TargetLanguageCode);
            _dbContext.LearnerLanguageProgress.Add(progress);
        }

        progress.RecordSession(message.DurationSeconds, message.OccurredAtUtc);
        _dbContext.InboxMessages.Add(new InboxMessage(
            context.MessageId,
            ConsumerName,
            context.ContractName,
            context.SchemaVersion,
            context.ReceivedAtUtc));
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
