using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Status of the confirm session usage operation.
    /// </summary>
    public enum ConfirmSessionUsageStatus
    {
        Success,
        ReservationNotFound,
        ReservationNotReserved,
        ReservationAlreadyConfirmed,
        ReservationAlreadyCancelled,
        ReservationExpired,
        InvalidDurationExceedsReserved,
        ConcurrencyConflict,
        UnknownFailure
    }

    /// <summary>
    /// Result of confirming session usage.
    /// </summary>
    public class ConfirmSessionUsageResult
    {
        public ConfirmSessionUsageResult(
            ConfirmSessionUsageStatus status,
            int remainingMinutes,
            int remainingSessions)
        {
            Status = status;
            RemainingMinutes = remainingMinutes;
            RemainingSessions = remainingSessions;
        }

        public ConfirmSessionUsageStatus Status { get; }
        public int RemainingMinutes { get; }
        public int RemainingSessions { get; }
    }
}