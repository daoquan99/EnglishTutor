using EnglishTutor.Quota.Contracts.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Quota.Contracts
{
    /// <summary>
    /// Contract for the Quota module.
    /// </summary>
    public interface IQuotaModule
    {
        /// <summary>
        /// Reserves quota for a session.
        /// </summary>
        Task<ReserveSessionQuotaResult> ReserveSessionQuotaAsync(
            ReserveSessionQuotaRequest request,
            CancellationToken ct);

        /// <summary>
        /// Confirms session usage and creates a usage log.
        /// </summary>
        Task<ConfirmSessionUsageResult> ConfirmSessionUsageAsync(
            ConfirmSessionUsageRequest request,
            CancellationToken ct);

        /// <summary>
        /// Cancels a reservation and releases the reserved quota.
        /// </summary>
        Task<CancelReservationResult> CancelReservationAsync(
            CancelReservationRequest request,
            CancellationToken ct);
    }
}