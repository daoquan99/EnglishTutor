using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IRateLimitEventRepository.
/// </summary>
public class RateLimitEventRepository : IRateLimitEventRepository
{
    private readonly QuotaDbContext _dbContext;

    public RateLimitEventRepository(QuotaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(RateLimitEvent rateLimitEvent, CancellationToken ct = default)
    {
        await _dbContext.RateLimitEvents.AddAsync(rateLimitEvent, ct);
    }
}
