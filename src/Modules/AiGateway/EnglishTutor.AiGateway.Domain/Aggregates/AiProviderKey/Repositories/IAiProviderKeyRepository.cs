using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;

/// <summary>
/// Repository interface for AiProviderKey aggregate.
/// </summary>
public interface IAiProviderKeyRepository
{
    Task<AiProviderKey?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AiProviderKey key, CancellationToken ct = default);
    void Update(AiProviderKey key);
}
