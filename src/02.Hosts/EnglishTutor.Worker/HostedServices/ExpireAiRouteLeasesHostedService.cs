using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using EnglishTutor.AiGateway.Application.RouteLeases.Commands.ExpireAiRouteLeases;
using EnglishTutor.Worker.Options;

namespace EnglishTutor.Worker.HostedServices;

/// <summary>
/// Background service that periodically cleans up expired AI route leases by dispatching the command.
/// </summary>
public class ExpireAiRouteLeasesHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireAiRouteLeasesHostedService> _logger;
    private readonly WorkerOptions _options;

    public ExpireAiRouteLeasesHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpireAiRouteLeasesHostedService> logger,
        IOptions<WorkerOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.JobsEnabled || !_options.EnableAiRouteLeaseExpiry)
        {
            _logger.LogInformation("Expired AI route leases cleanup hosted service is disabled.");
            return;
        }

        _logger.LogInformation("Expired AI route leases cleanup hosted service starting. Run interval: {Interval}.", _options.AiRouteLeaseExpiryInterval);

        using var timer = new PeriodicTimer(_options.AiRouteLeaseExpiryInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessExpiredLeasesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Expired AI route leases cleanup hosted service is shutting down.");
        }
    }

    private async Task ProcessExpiredLeasesAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting to process expired AI route leases via Application command...");

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var command = new ExpireAiRouteLeasesCommand(_options.AiRouteLeaseExpiryBatchSize);
            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Finished processing expired AI route leases. Expired count: {Count}", result.Value);
            }
            else
            {
                _logger.LogError("Expired AI route leases cleanup command failed: {ErrorCode} - {ErrorMessage}", result.Error?.Code, result.Error?.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred during expired AI route lease cleanup hosted service execution.");
        }
    }
}
