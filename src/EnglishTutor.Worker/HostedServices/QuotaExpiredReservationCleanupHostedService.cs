using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EnglishTutor.Quota.Application;
using EnglishTutor.Quota.Application.Abstractions.Persistence;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;

namespace EnglishTutor.Worker.HostedServices;

/// <summary>
/// Background service that periodically cleans up expired quota reservations.
/// </summary>
public class QuotaExpiredReservationCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuotaExpiredReservationCleanupHostedService> _logger;
    private readonly QuotaOptions _options;

    public QuotaExpiredReservationCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<QuotaExpiredReservationCleanupHostedService> logger,
        IOptions<QuotaOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Quota expired reservation cleanup hosted service starting. Run interval: {Interval} seconds.", _options.ExpiredReservationCleanupIntervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.ExpiredReservationCleanupIntervalSeconds));

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
        _logger.LogInformation("Starting to process expired quota reservations.");

        var now = DateTime.UtcNow;
        var batchSize = _options.ExpiredReservationCleanupBatchSize;

        // We'll process in batches until no more expired reservations are found.
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IQuotaUnitOfWork>();

            // Get a batch of expired reservations
            var expiredReservations = await unitOfWork.QuotaReservations.GetExpiredReservationsAsync(now, batchSize, ct);
            if (expiredReservations == null || expiredReservations.Count == 0)
            {
                break;
            }

            _logger.LogInformation("Found {Count} expired reservations to process.", expiredReservations.Count);

            foreach (var reservation in expiredReservations)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                await ProcessSingleReservationAsync(unitOfWork, reservation, now, ct);
            }
        }

        _logger.LogInformation("Finished processing expired quota reservations.");
    }

    private async Task ProcessSingleReservationAsync(IQuotaUnitOfWork unitOfWork, QuotaReservation reservation, DateTime now, CancellationToken ct)
    {
        // We'll attempt to process the reservation with retries for concurrency conflicts.
        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                // Reload the reservation to get the latest version and check if it's still expired and reserved.
                var latestReservation = await unitOfWork.QuotaReservations.GetByIdAsync(reservation.Id, ct);
                if (latestReservation == null)
                {
                    // Already deleted (unlikely because we soft delete).
                    return;
                }

                if (latestReservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    // No longer reserved (maybe already confirmed, cancelled, or expired by another instance).
                    return;
                }

                if (latestReservation.ExpiresAtUtc >= now)
                {
                    // Not expired anymore (should not happen if we queried for expired, but just in case).
                    return;
                }

                // Mark the reservation as expired.
                latestReservation.Expire(now);

                // Get the user quota state for this reservation.
                var state = await unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
                    latestReservation.UserId, latestReservation.QuotaDate, ct);
                if (state == null)
                {
                    _logger.LogWarning("Could not find UserQuotaState for UserId {UserId} and QuotaDate {QuotaDate}. Skipping reservation {ReservationId}.",
                        latestReservation.UserId, latestReservation.QuotaDate, latestReservation.Id);
                    return;
                }

                // Update the state: consume the reserved minutes and decrement the reserved session count.
                state.ConsumeReservedMinutes(latestReservation.RequestedMinutes);
                state.DecrementReservedSessionCount();

                // Persist changes.
                unitOfWork.QuotaReservations.Update(latestReservation);
                unitOfWork.UserQuotaStates.Update(state);
                await unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully expired reservation {ReservationId} for user {UserId}.", reservation.Id, reservation.UserId);
                return; // Success
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    _logger.LogWarning(ex, "Failed to expire reservation {ReservationId} after {MaxRetries} attempts due to concurrency conflict.",
                        reservation.Id, _options.MaxConcurrencyRetryCount);
                    // Give up on this reservation; it will be retried in the next cycle.
                    return;
                }
                // Otherwise, retry (loop continues)
                await Task.Delay(100, ct); // Optional delay before retry
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing reservation {ReservationId}.", reservation.Id);
                // Unexpected error, we break to avoid infinite loop.
                return;
            }
        }
    }
}