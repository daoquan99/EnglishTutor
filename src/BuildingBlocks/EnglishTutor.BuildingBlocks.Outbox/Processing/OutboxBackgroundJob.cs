using Microsoft.Extensions.Logging;
using Quartz;

namespace EnglishTutor.BuildingBlocks.Outbox.Processing;

[DisallowConcurrentExecution]
public sealed class OutboxBackgroundJob(
    IOutboxProcessor outboxProcessor,
    ILogger<OutboxBackgroundJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Outbox processing started at {Time}", DateTime.UtcNow);

        await outboxProcessor.ProcessPendingMessagesAsync(context.CancellationToken);
    }
}
