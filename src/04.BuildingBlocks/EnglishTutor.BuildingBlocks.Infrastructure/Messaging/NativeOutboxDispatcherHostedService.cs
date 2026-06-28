using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class NativeOutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqPublisher _publisher;
    private readonly NativeMessagingOptions _options;
    private readonly ILogger<NativeOutboxDispatcherHostedService> _logger;

    public NativeOutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        RabbitMqPublisher publisher,
        IOptions<NativeMessagingOptions> options,
        ILogger<NativeOutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(_options.OutboxPollIntervalMilliseconds));
        do
        {
            try
            {
                await DispatchAvailableAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Native outbox dispatch cycle failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchAvailableAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var stores = scope.ServiceProvider.GetServices<IOutboxStore>();
        foreach (var store in stores)
        {
            var messages = await store.ClaimAsync(
                batchSize: _options.OutboxBatchSize,
                leaseDuration: TimeSpan.FromSeconds(_options.LeaseSeconds),
                cancellationToken: cancellationToken);
            foreach (var message in messages)
            {
                var lockId = message.LockId
                    ?? throw new InvalidOperationException("Claimed outbox message has no lock ID.");
                try
                {
                    await _publisher.PublishAsync(message, cancellationToken);
                    await store.MarkPublishedAsync(
                        messageId: message.Id,
                        lockId: lockId,
                        cancellationToken: cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Publishing outbox message {MessageId} from {Module} failed.",
                        message.Id,
                        store.ModuleName);
                    await store.MarkFailedAsync(
                        messageId: message.Id,
                        lockId: lockId,
                        errorCode: "rabbitmq_publish_failed",
                        errorMessage: exception.Message,
                        retryDelay: TimeSpan.FromSeconds(_options.PublishRetrySeconds),
                        maxAttempts: _options.MaxPublishAttempts,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
