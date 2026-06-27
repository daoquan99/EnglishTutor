using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Application.Abstractions.Persistence;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ExpireQuotaReservations;

public sealed class ExpireQuotaReservationsCommandHandler : ICommandHandler<ExpireQuotaReservationsCommand, int>
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly QuotaOptions _options;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ExpireQuotaReservationsCommandHandler> _logger;

    public ExpireQuotaReservationsCommandHandler(
        IQuotaUnitOfWork unitOfWork,
        IOptions<QuotaOptions> options,
        IDateTimeProvider clock,
        ILogger<ExpireQuotaReservationsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> Handle(ExpireQuotaReservationsCommand command, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var batchSize = command.BatchSize;

        var expiredReservations = await _unitOfWork.QuotaReservations.GetExpiredReservationsAsync(now, batchSize, ct);
        if (expiredReservations == null || expiredReservations.Count == 0)
        {
            return Result.Success(0);
        }

        _logger.LogInformation("Found {Count} expired reservations to process in Quota Application.", expiredReservations.Count);
        int successCount = 0;

        foreach (var reservation in expiredReservations)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var processed = await ProcessSingleReservationAsync(reservation, now, ct);
            if (processed)
            {
                successCount++;
            }
        }

        return Result.Success(successCount);
    }

    private async Task<bool> ProcessSingleReservationAsync(QuotaReservation reservation, DateTime now, CancellationToken ct)
    {
        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    return false;
                }

                var latestReservation = await _unitOfWork.QuotaReservations.GetByIdAsync(reservation.Id, ct);
                if (latestReservation == null)
                {
                    return false;
                }

                if (latestReservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    return false;
                }

                if (latestReservation.ExpiresAtUtc >= now)
                {
                    return false;
                }

                latestReservation.Expire(now);

                var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
                    latestReservation.UserId, latestReservation.QuotaDate, ct);
                if (state == null)
                {
                    _logger.LogWarning("Could not find UserQuotaState for UserId {UserId} and QuotaDate {QuotaDate}. Skipping reservation {ReservationId}.",
                        latestReservation.UserId, latestReservation.QuotaDate, latestReservation.Id);
                    return false;
                }

                state.ConsumeReservedMinutes(latestReservation.RequestedMinutes);
                state.DecrementReservedSessionCount();

                _unitOfWork.QuotaReservations.Update(latestReservation);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully expired reservation {ReservationId} for user {UserId} via Application command.", reservation.Id, reservation.UserId);
                return true; 
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    _logger.LogWarning(ex, "Failed to expire reservation {ReservationId} after {MaxRetries} attempts due to concurrency conflict.",
                        reservation.Id, _options.MaxConcurrencyRetryCount);
                    return false;
                }
                await Task.Delay(100, ct); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing reservation {ReservationId}.", reservation.Id);
                return false;
            }
        }
        return false;
    }
}
