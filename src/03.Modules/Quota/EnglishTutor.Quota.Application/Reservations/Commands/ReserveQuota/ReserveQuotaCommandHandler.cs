using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Application.Abstractions.Persistence;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ReserveQuota;

public sealed class ReserveQuotaCommandHandler : ICommandHandler<ReserveQuotaCommand, ReserveSessionQuotaResult>
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly QuotaOptions _options;
    private readonly IDateTimeProvider _clock;

    public ReserveQuotaCommandHandler(
        IQuotaUnitOfWork unitOfWork,
        IOptions<QuotaOptions> options,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Result<ReserveSessionQuotaResult>> Handle(ReserveQuotaCommand command, CancellationToken ct)
    {
        var request = command.Request;

        // Get the effective quota rule for the user
        var rule = await GetEffectiveRuleAsync(request.UserId, ct);
        if (rule == null)
        {
            return Result.Success(new ReserveSessionQuotaResult(
                null, 0, 0, null, ReserveSessionQuotaStatus.UnknownFailure));
        }

        // Get or create the user quota state for today (UTC date)
        var today = DateTime.SpecifyKind(_clock.UtcNow.Date, DateTimeKind.Utc);
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
            return Result.Success(new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.SingleSessionDurationExceeded));
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
            return Result.Success(new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.DailyMinutesExceeded));
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
            return Result.Success(new ReserveSessionQuotaResult(
                null,
                state.UsedMinutes,
                state.ReservedSessionCount,
                null,
                ReserveSessionQuotaStatus.DailySessionsExceeded));
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
            return Result.Success(new ReserveSessionQuotaResult(
                existingReservation.Id,
                existingRemainingMinutes,
                existingRemainingSessions,
                existingReservation.ExpiresAtUtc,
                ReserveSessionQuotaStatus.IdempotentRepeat));
        }

        // We need to create a new reservation. We'll do this in a retry loop for concurrency.
        Guid? reservationId = null;
        DateTime? expiresAtUtc = null;
        int remainingMinutes = 0;
        int remainingSessions = 0;

        var stateId = state.Id;
        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                // Re-fetch the state to get the latest version (in case it changed)
                if (!stateWasCreated)
                {
                    var fetchedState = await _unitOfWork.UserQuotaStates.GetByIdAsync(stateId, ct);
                    if (fetchedState == null)
                    {
                        return Result.Success(new ReserveSessionQuotaResult(
                            null, 0, 0, null, ReserveSessionQuotaStatus.ConcurrencyConflict));
                    }
                    state = fetchedState;
                }

                // Create the reservation
                var reservation = QuotaReservation.Create(
                    Guid.NewGuid(),
                    request.UserId,
                    request.IdempotencyKey,
                    request.RequestedMinutes,
                    _clock.UtcNow.AddMinutes(_options.ReservationTtlMinutes),
                    state!.QuotaDate,
                    request.CorrelationId,
                    request.PracticeSessionId);

                // Update the state
                state.AddReservedMinutes(request.RequestedMinutes);
                state.IncrementReservedSessionCount();

                // Persist changes
                await _unitOfWork.QuotaReservations.AddAsync(reservation, ct);
                if (!stateWasCreated)
                {
                    _unitOfWork.UserQuotaStates.Update(state);
                }
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
                    return Result.Success(new ReserveSessionQuotaResult(
                        null, 0, 0, null, ReserveSessionQuotaStatus.ConcurrencyConflict));
                }
                stateWasCreated = false; 
                reservationId = null;
                expiresAtUtc = null;
            }
        }

        if (reservationId == null)
        {
            return Result.Success(new ReserveSessionQuotaResult(
                null, 0, 0, null, ReserveSessionQuotaStatus.UnknownFailure));
        }

        return Result.Success(new ReserveSessionQuotaResult(
            reservationId,
            remainingMinutes,
            remainingSessions,
            expiresAtUtc,
            ReserveSessionQuotaStatus.Success));
    }

    private async Task<UserQuotaRule> GetEffectiveRuleAsync(Guid userId, CancellationToken ct)
    {
        var userRule = await _unitOfWork.UserQuotaRules.GetByUserIdAsync(userId, ct);
        if (userRule != null && userRule.IsActive)
        {
            return userRule;
        }

        var globalRules = await _unitOfWork.UserQuotaRules.GetAllActiveAsync(ct);
        var firstOrDefault = globalRules.FirstOrDefault();
        if (firstOrDefault != null)
        {
            return firstOrDefault;
        }

        return UserQuotaRule.Create(
            Guid.Empty,
            null,
            _options.DefaultDailyMaxSessionMinutes,
            _options.DefaultDailyMaxSessions,
            _options.DefaultMaxSingleSessionMinutes,
            _clock.UtcNow,
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
            _clock.UtcNow,
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
