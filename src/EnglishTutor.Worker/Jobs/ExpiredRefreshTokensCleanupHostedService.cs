using EnglishTutor.Identity.Application.Commands.PurgeExpiredRefreshTokens;
using EnglishTutor.Worker.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Worker.Jobs;

/// <summary>
/// A hosted service that runs periodically to purge expired refresh tokens from the database.
/// It uses a <see cref="PeriodicTimer"/> and sends the purging command through MediatR <see cref="ISender"/>.
/// </summary>
public sealed class ExpiredRefreshTokensCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredRefreshTokensCleanupHostedService> _logger;
    private readonly WorkerOptions _options;

    public ExpiredRefreshTokensCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredRefreshTokensCleanupHostedService> logger,
        IOptions<WorkerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.JobsEnabled || !_options.EnableExpiredRefreshTokenCleanup)
        {
            _logger.LogInformation("Expired refresh token cleanup background job is disabled.");
            return;
        }

        _logger.LogInformation(
            "Worker Background Jobs registered: Expired Refresh Tokens Cleanup (Interval: {Interval}, Retention: {Retention} days)",
            _options.RefreshTokenCleanupInterval,
            _options.RefreshTokenRetentionDays);

        // Run the first purge immediately on startup, then wait for subsequent timer ticks.
        await RunCleanupSafely(stoppingToken);

        using var timer = new PeriodicTimer(_options.RefreshTokenCleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCleanupSafely(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Expired refresh token cleanup background job is shutting down.");
        }
    }

    private async Task RunCleanupSafely(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting expired refresh tokens cleanup job...");

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var command = new PurgeExpiredRefreshTokensCommand(_options.RefreshTokenRetentionDays);
            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Expired refresh tokens cleanup job completed successfully. Purged {PurgedCount} tokens.", result.Value);
            }
            else
            {
                _logger.LogError("Expired refresh tokens cleanup job failed. Error: {ErrorCode} - {ErrorMessage}", result.Error?.Code, result.Error?.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Do not crash the worker on a single job iteration failure.
            _logger.LogError(ex, "An error occurred while executing expired refresh tokens cleanup job.");
        }
    }
}
