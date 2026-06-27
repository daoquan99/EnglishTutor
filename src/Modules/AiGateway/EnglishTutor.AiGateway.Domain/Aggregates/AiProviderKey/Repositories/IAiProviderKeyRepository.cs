using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;

/// <summary>
/// Repository interface for AiProviderKey aggregate.
/// </summary>
public interface IAiProviderKeyRepository
{
    Task<AiProviderKey?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>All keys for a provider (active and inactive), for admin listing.</summary>
    Task<IReadOnlyList<AiProviderKey>> ListByProviderAsync(Guid providerId, CancellationToken ct = default);

    /// <summary>
    /// Active keys for a provider ordered by ascending priority, used by route
    /// selection. Cooldown is evaluated by the caller against the current time.
    /// </summary>
    Task<IReadOnlyList<AiProviderKey>> ListActiveByProviderByPriorityAsync(Guid providerId, CancellationToken ct = default);

    Task AddAsync(AiProviderKey key, CancellationToken ct = default);
    void Update(AiProviderKey key);
}
