using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;

namespace EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;

/// <summary>
/// Repository interface for QuotaReservation aggregate.
/// </summary>
public interface IQuotaReservationRepository
{
    Task<QuotaReservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<QuotaReservation?> GetByIdempotencyKeyAsync(Guid userId, string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(QuotaReservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<QuotaReservation>> GetExpiredReservationsAsync(DateTime now, int batchSize, CancellationToken ct = default);
    void Update(QuotaReservation reservation);
}