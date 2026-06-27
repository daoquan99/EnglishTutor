using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using EnglishTutor.Quota.Application.Reservations.Commands.ExpireQuotaReservations;
using EnglishTutor.Worker.Options;

namespace EnglishTutor.Worker.HostedServices;

/// <summary>
/// Background service that periodically cleans up expired quota reservations by dispatching the command.
/// </summary>
public class QuotaExpiredReservationCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuotaExpiredReservationCleanupHostedService> _logger;
    private readonly WorkerOptions _options;

    public QuotaExpiredReservationCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<QuotaExpiredReservationCleanupHostedService> logger,
        IOptions<WorkerOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.JobsEnabled || !_options.EnableQuotaReservationExpiry)
        {
            _logger.LogInformation("Quota expired reservation cleanup hosted service is disabled.");
            return;
        }

        _logger.LogInformation("Quota expired reservation cleanup hosted service starting. Run interval: {Interval}.", _options.QuotaReservationExpiryInterval);

        using var timer = new PeriodicTimer(_options.QuotaReservationExpiryInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Quota expired reservation cleanup hosted service is shutting down.");
        }
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting to process expired quota reservations via Application command...");

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var command = new ExpireQuotaReservationsCommand(_options.QuotaReservationExpiryBatchSize);
            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Finished processing expired quota reservations. Expired count: {Count}", result.Value);
            }
            else
            {
                _logger.LogError("Expired quota reservations cleanup command failed: {ErrorCode} - {ErrorMessage}", result.Error?.Code, result.Error?.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred during expired quota reservation cleanup hosted service execution.");
        }
    }
}