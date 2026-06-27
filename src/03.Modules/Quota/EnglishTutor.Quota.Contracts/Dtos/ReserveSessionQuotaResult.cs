using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Status of the reservation operation.
    /// </summary>
    public enum ReserveSessionQuotaStatus
    {
        Success,
        DailyMinutesExceeded,
        DailySessionsExceeded,
        SingleSessionDurationExceeded,
        IdempotentRepeat,
        ConcurrencyConflict,
        UnknownFailure
    }

    /// <summary>
    /// Result of reserving session quota.
    /// </summary>
    public class ReserveSessionQuotaResult
    {
        public ReserveSessionQuotaResult(
            Guid? reservationId,
            int remainingMinutes,
            int remainingSessions,
            DateTime? expiresAtUtc,
            ReserveSessionQuotaStatus status)
        {
            ReservationId = reservationId;
            RemainingMinutes = remainingMinutes;
            RemainingSessions = remainingSessions;
            ExpiresAtUtc = expiresAtUtc;
            Status = status;
        }

        public Guid? ReservationId { get; }
        public int RemainingMinutes { get; }
        public int RemainingSessions { get; }
        public DateTime? ExpiresAtUtc { get; }
        public ReserveSessionQuotaStatus Status { get; }
    }
}