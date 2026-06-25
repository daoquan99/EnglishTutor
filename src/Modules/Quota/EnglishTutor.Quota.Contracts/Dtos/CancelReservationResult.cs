using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Result of canceling a reservation.
    /// </summary>
    public class CancelReservationResult
    {
        public CancelReservationResult(
            CancelReservationStatus status,
            int remainingMinutes,
            int remainingSessions)
        {
            Status = status;
            RemainingMinutes = remainingMinutes;
            RemainingSessions = remainingSessions;
        }

        public CancelReservationStatus Status { get; }
        public int RemainingMinutes { get; }
        public int RemainingSessions { get; }
    }

    /// <summary>
    /// Status of the cancel reservation operation.
    /// </summary>
    public enum CancelReservationStatus
    {
        Success,
        ReservationNotFound,
        ReservationNotReserved,
        ReservationAlreadyConfirmed,
        ReservationAlreadyCancelled,
        ReservationExpired,
        ConcurrencyConflict,
        UnknownFailure
    }
}