using EnglishTutor.BuildingBlocks.Outbox;
using Quartz;

namespace EnglishTutor.Worker.Jobs;

[DisallowConcurrentExecution]
public sealed class OutboxProcessingJob(
    IOutboxProcessor outboxProcessor,
    ILogger<OutboxProcessingJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Outbox processing started at {Time}", DateTime.UtcNow);

        await outboxProcessor.ProcessPendingMessagesAsync(context.CancellationToken);
    }
}
