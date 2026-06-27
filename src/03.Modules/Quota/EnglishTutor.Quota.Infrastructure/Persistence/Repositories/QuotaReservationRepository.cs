using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Infrastructure.Persistence;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IQuotaReservationRepository.
/// </summary>
public class QuotaReservationRepository : IQuotaReservationRepository
{
    private readonly QuotaDbContext _dbContext;

    public QuotaReservationRepository(QuotaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<QuotaReservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.QuotaReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<QuotaReservation?> GetByIdempotencyKeyAsync(Guid userId, string idempotencyKey, CancellationToken ct = default)
    {
        return await _dbContext.QuotaReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task AddAsync(QuotaReservation reservation, CancellationToken ct = default)
    {
        await _dbContext.QuotaReservations.AddAsync(reservation, ct);
    }

    public async Task<IReadOnlyList<QuotaReservation>> GetExpiredReservationsAsync(DateTime now, int batchSize, CancellationToken ct = default)
    {
        return await _dbContext.QuotaReservations
            .Where(r => r.Status == QuotaReservation.ReservationStatus.Reserved && r.ExpiresAtUtc < now)
            .OrderBy(r => r.ExpiresAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void Update(QuotaReservation reservation)
    {
        _dbContext.QuotaReservations.Update(reservation);
    }
}