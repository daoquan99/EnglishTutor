using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;

namespace EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;

/// <summary>
/// Repository interface for RateLimitEvent aggregate.
/// </summary>
public interface IRateLimitEventRepository
{
    Task AddAsync(RateLimitEvent rateLimitEvent, CancellationToken ct = default);
}