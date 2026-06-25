using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;

/// <summary>
/// Repository interface for AiRouteLease aggregate.
/// </summary>
public interface IAiRouteLeaseRepository
{
    Task<AiRouteLease?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AiRouteLease?> GetByIdempotencyKeyAsync(Guid userId, string idempotencyKey, CancellationToken ct = default);
    Task AddAsync(AiRouteLease lease, CancellationToken ct = default);
    void Update(AiRouteLease lease);
}
