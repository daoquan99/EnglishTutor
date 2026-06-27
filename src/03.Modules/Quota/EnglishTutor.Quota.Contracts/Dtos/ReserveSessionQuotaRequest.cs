using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Request to reserve quota for a session.
    /// </summary>
    public class ReserveSessionQuotaRequest
    {
        /// <summary>
        /// The user identifier.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The requested minutes for the session.
        /// </summary>
        public int RequestedMinutes { get; set; }

        /// <summary>
        /// Idempotency key to ensure idempotent requests.
        /// </summary>
        public string IdempotencyKey { get; set; } = default!;

        /// <summary>
        /// Correlation ID for tracing.
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// Optional practice session ID if this reservation is for a practice session.
        /// </summary>
        public Guid? PracticeSessionId { get; set; }
    }
}