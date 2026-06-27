using System;

namespace EnglishTutor.Quota.Contracts.Dtos
{
    /// <summary>
    /// Request to cancel a reservation.
    /// </summary>
    public class CancelReservationRequest
    {
        /// <summary>
        /// The reservation identifier.
        /// </summary>
        public Guid ReservationId { get; set; }
    }
}