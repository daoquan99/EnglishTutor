using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Request to confirm session usage.
    /// </summary>
    public class ConfirmSessionUsageRequest
    {
        /// <summary>
        /// The reservation identifier.
        /// </summary>
        public Guid ReservationId { get; set; }

        /// <summary>
        /// The actual minutes used in the session.
        /// </summary>
        public int ActualMinutesUsed { get; set; }

        /// <summary>
        /// Correlation ID for tracing.
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// Optional practice session ID if this confirmation is for a practice session.
        /// </summary>
        public Guid? PracticeSessionId { get; set; }
    }
}