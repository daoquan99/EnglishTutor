using EnglishTutor.BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EnglishTutor.BuildingBlocks.Outbox.Processing;

[DisallowConcurrentExecution]
public sealed class OutboxBackgroundJob(
    IOutboxProcessor outboxProcessor,
    ILogger<OutboxBackgroundJob> logger,
    IDateTimeProvider dateTimeProvider) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Outbox processing started at {Time}", dateTimeProvider.UtcNow);

        await outboxProcessor.ProcessPendingMessagesAsync(context.CancellationToken);
    }
}
