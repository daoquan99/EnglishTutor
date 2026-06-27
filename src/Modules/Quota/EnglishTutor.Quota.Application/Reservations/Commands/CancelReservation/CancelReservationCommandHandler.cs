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
using EnglishTutor.Quota.Application.Abstractions.Persistence;

namespace EnglishTutor.Quota.Application.Reservations.Commands.CancelReservation;

public sealed class CancelReservationCommandHandler : ICommandHandler<CancelReservationCommand, CancelReservationResult>
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly QuotaOptions _options;
    private readonly IDateTimeProvider _clock;

    public CancelReservationCommandHandler(
        IQuotaUnitOfWork unitOfWork,
        IOptions<QuotaOptions> options,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Result<CancelReservationResult>> Handle(CancelReservationCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var reservation = await _unitOfWork.QuotaReservations.GetByIdAsync(request.ReservationId, ct);
        if (reservation == null)
        {
            return Result.Success(new CancelReservationResult(CancelReservationStatus.ReservationNotFound, 0, 0));
        }

        if (reservation.Status != QuotaReservation.ReservationStatus.Reserved)
        {
            return Result.Success(reservation.Status switch
            {
                QuotaReservation.ReservationStatus.Confirmed => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyConfirmed, 0, 0),
                QuotaReservation.ReservationStatus.Cancelled => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyCancelled, 0, 0),
                QuotaReservation.ReservationStatus.Expired => new CancelReservationResult(CancelReservationStatus.ReservationExpired, 0, 0),
                _ => new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0)
            });
        }

        var state = await _unitOfWork.UserQuotaStates.GetByUserIdAndQuotaDateAsync(
            reservation.UserId, reservation.QuotaDate, ct);
        if (state == null)
        {
            return Result.Success(new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0));
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
                    return Result.Success(new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0));
                }
                if (fetchedReservation.Status != QuotaReservation.ReservationStatus.Reserved)
                {
                    return Result.Success(fetchedReservation.Status switch
                    {
                        QuotaReservation.ReservationStatus.Confirmed => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyConfirmed, 0, 0),
                        QuotaReservation.ReservationStatus.Cancelled => new CancelReservationResult(CancelReservationStatus.ReservationAlreadyCancelled, 0, 0),
                        QuotaReservation.ReservationStatus.Expired => new CancelReservationResult(CancelReservationStatus.ReservationExpired, 0, 0),
                        _ => new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0)
                    });
                }
                reservation = fetchedReservation;

                var fetchedState = await _unitOfWork.UserQuotaStates.GetByIdAsync(stateId, ct);
                if (fetchedState == null)
                {
                    return Result.Success(new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0));
                }
                state = fetchedState;

                reservation.Cancel(_clock.UtcNow);
                state.ConsumeReservedMinutes(reservation.RequestedMinutes);
                state.DecrementReservedSessionCount();

                _unitOfWork.QuotaReservations.Update(reservation);
                _unitOfWork.UserQuotaStates.Update(state);
                await _unitOfWork.SaveChangesAsync(ct);

                var rule = await GetEffectiveRuleAsync(reservation.UserId, ct);
                if (rule == null)
                {
                    return Result.Success(new CancelReservationResult(CancelReservationStatus.Success, 0, 0));
                }

                var remainingMinutes = rule.DailyMaxSessionMinutes - (state.UsedMinutes + state.ReservedMinutes);
                var remainingSessions = rule.DailyMaxSessions - (state.ReservedSessionCount + state.UsedSessionCount);

                return Result.Success(new CancelReservationResult(
                    CancelReservationStatus.Success,
                    remainingMinutes < 0 ? 0 : remainingMinutes,
                    remainingSessions < 0 ? 0 : remainingSessions));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == _options.MaxConcurrencyRetryCount)
                {
                    return Result.Success(new CancelReservationResult(CancelReservationStatus.ConcurrencyConflict, 0, 0));
                }
                continue;
            }
        }

        return Result.Success(new CancelReservationResult(CancelReservationStatus.UnknownFailure, 0, 0));
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
