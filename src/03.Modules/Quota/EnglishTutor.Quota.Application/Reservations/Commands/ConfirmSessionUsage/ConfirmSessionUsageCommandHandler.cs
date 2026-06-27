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
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Application.Abstractions.Persistence;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ConfirmSessionUsage;

public sealed class ConfirmSessionUsageCommandHandler : ICommandHandler<ConfirmSessionUsageCommand, ConfirmSessionUsageResult>
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly QuotaOptions _options;
    private readonly IDateTimeProvider _clock;

    public ConfirmSessionUsageCommandHandler(
        IQuotaUnitOfWork unitOfWork,
        IOptions<QuotaOptions> options,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Result<ConfirmSessionUsageResult>> Handle(ConfirmSessionUsageCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(request.ReservationId, ct);
        if (reservation == null)
        {
            return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationNotFound, 0, 0));
        }

        if (reservation.Status != QuotaReservation.ReservationStatus.Reserved)
        {
            return Result.Success(reservation.Status switch
            {
                QuotaReservation.ReservationStatus.Confirmed => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyConfirmed, 0, 0),
                QuotaReservation.ReservationStatus.Cancelled => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyCancelled, 0, 0),
                QuotaReservation.ReservationStatus.Expired => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationExpired, 0, 0),
                _ => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0)
            });
        }

        if (request.ActualMinutesUsed > reservation.RequestedMinutes)
        {
            return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.InvalidDurationExceedsReserved, 0, 0));
        }

        var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
            reservation.UserId, reservation.QuotaDate, ct);
        if (state == null)
        {
            return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0));
        }

        var reservationId = reservation.Id;
        var stateId = state.Id;

        for (int attempt = 0; attempt <= _options.MaxConcurrencyRetryCount; attempt++)
        {
            try
            {
                var fetchedReservation = await _unitOfWork.QuotaReservations.GetByIdAsync(reservationId, ct);
                if (fetchedReservation == null)
                {
                    return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0));
                }
                if (fetchedReservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    return Result.Success(fetchedReservation.Status switch
                    {
                        QuotaReservation.ReservationStatus.Confirmed => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyConfirmed, 0, 0),
                        QuotaReservation.ReservationStatus.Cancelled => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationAlreadyCancelled, 0, 0),
                        QuotaReservation.ReservationStatus.Expired => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ReservationExpired, 0, 0),
                        _ => new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0)
                    });
                }
                reservation = fetchedReservation;

                var fetchedState = await _unitOfWork.UserQuotaStates.GetByIdAsync(stateId, ct);
                if (fetchedState == null)
                {
                    return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0));
                }
                state = fetchedState;

                reservation.Confirm(_clock.UtcNow);
                state.ConsumeReservedMinutes(reservation.RequestedMinutes);
                state.DecrementReservedSessionCount();
                state.AddUsedMinutes(request.ActualMinutesUsed);
                state.IncrementUsedSessionCount();

                var usageLog = UsageLog.Create(
                    Guid.NewGuid(),
                    reservation.UserId,
                    reservation.Id,
                    request.ActualMinutesUsed,
                    _clock.UtcNow,
                    request.PracticeSessionId.HasValue ? "Practice" : "System",
                    request.PracticeSessionId);

                _unitOfWork.QuotaReservations.Update(reservation);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.UsageLogs.AddAsync(usageLog, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var rule = await GetEffectiveRuleAsync(reservation.UserId, ct);
                if (rule == null)
                {
                    return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.Success, 0, 0));
                }

                var remainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
                var remainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);

                return Result.Success(new ConfirmSessionUsageResult(
                    ConfirmSessionUsageStatus.Success,
                    remainingMinutes < 0 ? 0 : remainingMinutes,
                    remainingSessions < 0 ? 0 : remainingSessions));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.ConcurrencyConflict, 0, 0));
                }
                continue;
            }
        }

        return Result.Success(new ConfirmSessionUsageResult(ConfirmSessionUsageStatus.UnknownFailure, 0, 0));
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
}
