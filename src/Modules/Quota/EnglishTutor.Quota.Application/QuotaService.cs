using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.Quota.Domain;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Application.Abstractions.Persistence;

namespace EnglishTutor.Quota.Application;

/// <summary>
/// Service implementing the quota module operations.
/// </summary>
public class QuotaService : IQuotaModule
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly QuotaOptions _options;

    public QuotaService(IQuotaUnitOfWork unitOfWork, IOptions<QuotaOptions> options)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ReserveSessionQuotaResult> ReserveSessionQuotaAsync(
        ReserveSessionQuotaRequest request,
        CancellationToken ct)
    {
        // Validate request
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.RequestedMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.RequestedMinutes));
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("IdempotencyKey is required.", nameof(request.IdempotencyKey));

        // Get the effective quota rule for the user
        var rule = await GetEffectiveRuleAsync(request.UserId, ct);
        if (rule == null)
        {
            // This should not happen if we have fallback options, but just in case.
            return new ReserveSessionQuotaResult(
                null, 0, 0, null, ReserveSessionQuotaStatus.UnknownFailure);
        }

        // Get or create the user quota state for today (UTC date)
        var today = DateTime.UtcNow.Date;
        var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
            request.UserId, today, ct);
        bool stateWasCreated = false;
        if (state == null)
        {
            state = UserQuotaState.Create(
                Guid.NewGuid(),
                request.UserId,
                today,
                0, // ReservedMinutes
                0, // UsedMinutes
                0, // ReservedSessionCount
                0, // UsedSessionCount
                0  // Initial version
            );
            await _unitOfWork.UserQuotaStates.AddAsync(state, ct);
            stateWasCreated = true;
        }

        // Check quotas
        if (request.RequestedMinutes > rule.MaxSingleSessionDuration)
        {
            await RecordRateLimitEventIfNeededAsync(
                request.UserId,
                request.RequestedMinutes,
                RuleLimitType.SingleSessionDurationExceeded,
                rule.MaxSingleSessionDuration,
                request.CorrelationId,
                ct);
            return new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.SingleSessionDurationExceeded);
        }

        if (state.UsedMinutes + state.ReservedMinutes + request.RequestedMinutes > rule.DailyMaxSessionMinutes)
        {
            await RecordRateLimitEventIfNeededAsync(
                request.UserId,
                request.RequestedMinutes,
                RuleLimitType.DailyMinutesExceeded,
                rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes),
                request.CorrelationId,
                ct);
            return new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.DailyMinutesExceeded);
        }

        if (state.ReservedSessionCount + state.UsedSessionCount + 1 > rule.DailyMaxSessions)
        {
            await RecordRateLimitEventIfNeededAsync(
                request.UserId,
                request.RequestedMinutes,
                RuleLimitType.DailySessionsExceeded,
                rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount),
                request.CorrelationId,
                ct);
            return new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.DailySessionsExceeded);
        }

        // Check for idempotent reservation (if exists and is either Reserved or Confirmed)
        var existingReservation = await _unitOfWork.QuotaReservations
            .GetByIdempotencyKeyAsync(request.UserId, request.IdempotencyKey, ct);
        if (existingReservation != null &&
            (existingReservation.Status == QuotaReservation.ReservationStatus.Reserved ||
             existingReservation.Status == QuotaReservation.ReservationStatus.Confirmed))
        {
            // Return the existing reservation as idempotent repeat
            var existingRemainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
            var existingRemainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);
            return new ReserveSessionQuotaResult(
                existingReservation.Id,
                existingRemainingMinutes,
                existingRemainingSessions,
                existingReservation.ExpiresAtUtc,
                ReserveSessionQuotaStatus.IdempotentRepeat);
        }

        // We need to create a new reservation. We'll do this in a retry loop for concurrency.
        QuotaReservation.ReservationStatus finalStatus = QuotaReservation.ReservationStatus.Reserved;
        Guid? reservationId = null;
        DateTime? expiresAtUtc = null;
        int remainingMinutes = 0;
        int remainingSessions = 0;

        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                // Re-fetch the state to get the latest version (in case it changed)
                if (!stateWasCreated)
                {
                    state = await _unitOfWork.UserQuotaStates.GetByIdAsync(state.Id, ct);
                    if (state == null)
                    {
                        // This should not happen, but if it does, we treat as conflict.
                        return new ReserveSessionQuotaResult(
                            null, 0, 0, null, ReserveSessionQuotaStatus.ConcurrencyConflict);
                    }
                }

                // Create the reservation
                var reservation = QuotaReservation.Create(
                    Guid.NewGuid(),
                    request.UserId,
                    request.IdempotencyKey,
                    request.RequestedMinutes,
                    DateTime.UtcNow.AddMinutes(_options.ReservationTtlMinutes),
                    state.QuotaDate,
                    request.CorrelationId,
                    request.PracticeSessionId);

                // Update the state
                state.AddReservedMinutes(request.RequestedMinutes);
                state.IncrementReservedSessionCount();

                // Persist changes
                await _unitOfWork.QuotaReservations.AddAsync(reservation, ct);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.SaveChangesAsync(ct);

                // If we get here, we succeeded
                reservationId = reservation.Id;
                expiresAtUtc = reservation.ExpiresAtUtc;
                remainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
                remainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);
                break;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    // We've exhausted retries
                    return new ReserveSessionQuotaResult(
                        null, 0, 0, null, ReserveSessionQuotaStatus.ConcurrencyConflict);
                }
                // Otherwise, we'll retry after reloading the state (which we do at the start of the loop)
                // Note: We don't need to do anything special here because we reload the state at the beginning of each loop.
                // However, we must ensure that the state variable is updated for the next iteration.
                // We'll set a flag to indicate that we need to reload the state in the next iteration.
                stateWasCreated = false; // So that we reload the state in the next iteration
                // Also, we need to reset the reservationId and expiresAtUtc because they are not set in this attempt.
                reservationId = null;
                expiresAtUtc = null;
                // Continue to the next iteration
            }
        }

        if (reservationId == null)
        {
            // This should not happen because we break on success or return on failure
            return new ReserveSessionQuotaResult(
                null, 0, 0, null, ReserveSessionQuotaStatus.UnknownFailure);
        }

        return new ReserveSessionQuotaResult(
            reservationId,
            remainingMinutes,
            remainingSessions,
            expiresAtUtc,
            ReserveSessionQuotaStatus.Success);
    }

    public async Task<ConfirmSessionUsageResult> ConfirmSessionUsageAsync(
        ConfirmSessionUsageRequest request,
        CancellationToken ct)
    {
        // Validate request
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.ActualMinutesUsed <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.ActualMinutesUsed));
        if (request.ReservationId == Guid.Empty)
            throw new ArgumentException("ReservationId is required.", nameof(request.ReservationId));

        // Get the reservation
        var reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(request.ReservationId, ct);
        if (reservation == null)
        {
            return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationNotFound, 0, 0);
        }

        // Handle idempotent cases based on current status
        if (reservation.Status != QuotaReservation.ReservationStatus.Reserved)
        {
            return reservation.Status switch
            {
                QuotaReservation.ReservationStatus.Confirmed => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyConfirmed, 0, 0),
                QuotaReservation.ReservationStatus.Cancelled => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyCancelled, 0, 0),
                QuotaReservation.ReservationStatus.Expired => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationExpired, 0, 0),
                _ => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0)
            };
        }

        // Validate actual minutes used
        if (request.ActualMinutesUsed > reservation.RequestedMinutes)
        {
            return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.InvalidDurationExceedsReserved, 0, 0);
        }

        // Get the user quota state for the reservation's user and quota date
        var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
            reservation.UserId, reservation.QuotaDate, ct);
        if (state == null)
        {
            // This should not happen if the reservation was created correctly.
            return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0);
        }

        // We'll attempt to update the reservation and state in a retry loop for concurrency.
        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                // Re-fetch the reservation and state to get latest versions
                reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(reservation.Id, ct);
                if (reservation == null || reservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    // If it's no longer reserved, treat as idempotent based on current status
                    return reservation.Status switch
                    {
                        QuotaReservation.ReservationStatus.Confirmed => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyConfirmed, 0, 0),
                        QuotaReservation.ReservationStatus.Cancelled => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyCancelled, 0, 0),
                        QuotaReservation.ReservationStatus.Expired => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationExpired, 0, 0),
                        _ => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0)
                    };
                }

                state = await _unitOfWork.UserQuotaStates.GetByIdAsync(state.Id, ct);
                if (state == null)
                {
                    return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0);
                }

                // Perform the confirmation
                reservation.Confirm(DateTime.UtcNow);
                state.ConsumeReservedMinutes(reservation.RequestedMinutes);
                state.DecrementReservedSessionCount();
                state.AddUsedMinutes(request.ActualMinutesUsed);
                state.IncrementUsedSessionCount();

                // Create usage log
                var usageLog = UsageLog.Create(
                    Guid.NewGuid(),
                    reservation.UserId,
                    reservation.Id,
                    request.ActualMinutesUsed,
                    DateTime.UtcNow,
                    request.PracticeSessionId.HasValue ? "Practice" : "System",
                    request.PracticeSessionId);

                // Persist changes
                _unitOfWork.QuotaReservations.Update(reservation);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.UsageLogs.AddAsync(usageLog, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                // Calculate remaining quotas
                // Note: We need to get the rule again to compute remaining minutes/sessions.
                // But we can compute from state and rule? We don't have the rule here.
                // We'll get the effective rule again (though it shouldn't have changed in a way that affects the limits).
                var rule = await GetEffectiveRuleAsync(reservation.UserId, ct);
                if (rule == null)
                {
                    // Fallback to zeros if we can't get the rule.
                    return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.Success, 0, 0);
                }

                var remainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
                var remainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);

                return new ConfirmSessionUsageResult(
                    ConfirmSessionUsageStatus.Success,
                    remainingMinutes < 0 ? 0 : remainingMinutes,
                    remainingSessions < 0 ? 0 : remainingSessions);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ConcurrencyConflict, 0, 0);
                }
                // Otherwise, retry (the loop will re-fetch the entities)
                continue;
            }
        }

        // Should not reach here
        return new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0);
    }

    public async Task<CancelReservationResult> CancelReservationAsync(
        CancelReservationRequest request,
        CancellationToken ct)
    {
        // Validate request
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.ReservationId == Guid.Empty)
            throw new ArgumentException("ReservationId is required.", nameof(request.ReservationId));

        // Get the reservation
        var reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(request.ReservationId, ct);
        if (reservation == null)
        {
            return new CancelReservationResult(CancelReservationStatus.ReservationNotFound, 0, 0);
        }

        // Handle idempotent cases based on current status
        if (reservation.Status != QuotaReservation.ReservationStatus.Reserved)
        {
            return reservation.Status switch
            {
                QuotaReservation.ReservationStatus.Confirmed => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyConfirmed, 0, 0),
                QuotaReservation.ReservationStatus.Cancelled => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyCancelled, 0, 0),
                QuotaReservation.ReservationStatus.Expired => new CancelReservationResult(CancelReservationStatus.ReservationExpired, 0, 0),
                _ => new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0)
            };
        }

        // Get the user quota state for the reservation's user and quota date
        var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
            reservation.UserId, reservation.QuotaDate, ct);
        if (state == null)
        {
            // This should not happen if the reservation was created correctly.
            return new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0);
        }

        // We'll attempt to cancel the reservation and update state in a retry loop for concurrency.
        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                // Re-fetch the reservation and state to get latest versions
                reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(reservation.Id, ct);
                if (reservation == null || reservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    // If it's no longer reserved, treat as idempotent based on current status
                    return reservation.Status switch
                    {
                        QuotaReservation.ReservationStatus.Confirmed => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyConfirmed, 0, 0),
                        QuotaReservation.ReservationStatus.Cancelled => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyCancelled, 0, 0),
                        QuotaReservation.ReservationStatus.Expired => new CancelReservationResult(CancelReservationStatus.ReservationExpired, 0, 0),
                        _ => new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0)
                    };
                }

                state = await _unitOfWork.UserQuotaStates.GetByIdAsync(state.Id, ct);
                if (state == null)
                {
                    return new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0);
                }

                // Perform the cancellation
                reservation.Cancel(DateTime.UtcNow);
                state.ConsumeReservedMinutes(reservation.RequestedMinutes);
                state.DecrementReservedSessionCount();

                // Persist changes
                _unitOfWork.QuotaReservations.Update(reservation);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.SaveChangesAsync(ct);

                // Calculate remaining quotas
                var rule = await GetEffectiveRuleAsync(reservation.UserId, ct);
                if (rule == null)
                {
                    // Fallback to zeros if we can't get the rule.
                    return new CancelReservationResult(CancelReservationStatus.Success, 0, 0);
                }

                var remainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
                var remainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);

                return new CancelReservationResult(
                    CancelReservationStatus.Success,
                    remainingMinutes < 0 ? 0 : remainingMinutes,
                    remainingSessions < 0 ? 0 : remainingSessions);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    return new CancelReservationResult(CancelReservationStatus.ConcurrencyConflict, 0, 0);
                }
                // Otherwise, retry (the loop will re-fetch the entities)
                continue;
            }
        }

        // Should not reach here
        return new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0);
    }

    private async Task<UserQuotaRule> GetEffectiveRuleAsync(Guid userId, CancellationToken ct)
    {
        // Try to get user-specific active rule
        var userRule = await _unitOfWork.UserQuotaRules.GetByUserIdAsync(userId, ct);
        if (userRule != null && userRule.IsActive)
        {
            return userRule;
        }

        // Try to get global active rule (UserId == null)
        var globalRules = await _unitOfWork.UserQuotaRules.GetAllActiveAsync(ct);
        var firstOrDefault = globalRules.FirstOrDefault();
        if (firstOrDefault != null)
        {
            return firstOrDefault;
        }

        // Fallback to options
        return UserQuotaRule.Create(
            Guid.Empty,
            null,
            _options.DefaultDailyMaxSessionMinutes,
            _options.DefaultDailyMaxSessions,
            _options.DefaultMaxSingleSessionMinutes,
            DateTime.UtcNow,
            true
        );
    }

    private async Task RecordRateLimitEventIfNeededAsync(
        Guid userId,
        int attemptedAmount,
        RuleLimitType limitType,
        int remainingAllowed,
        Guid? correlationId,
        CancellationToken ct)
    {
        // Only record if the limit type is one of the three that trigger a rate limit event
        if (limitType == RuleLimitType.None)
            return;

        var limitTypeEnum = limitType switch
        {
            RuleLimitType.DailyMinutesExceeded => RateLimitEvent.LimitTypeEnum.DailyMinutesExceeded,
            RuleLimitType.DailySessionsExceeded => RateLimitEvent.LimitTypeEnum.DailySessionsExceeded,
            RuleLimitType.SingleSessionDurationExceeded => RateLimitEvent.LimitTypeEnum.SingleSessionDurationExceeded,
            _ => throw new ArgumentOutOfRangeException(nameof(limitType))
        };

        var rateLimitEvent = RateLimitEvent.Create(
            Guid.NewGuid(),
            userId,
            $"Quota exceeded: {limitType}",
            limitTypeEnum,
            attemptedAmount,
            DateTime.UtcNow,
            correlationId);

        await _unitOfWork.RateLimitEvents.AddAsync(rateLimitEvent, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private enum RuleLimitType
    {
        None,
        DailyMinutesExceeded,
        DailySessionsExceeded,
        SingleSessionDurationExceeded
    }
}