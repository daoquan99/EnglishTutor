using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;

/// <summary>
/// Repository interface for AiProvider aggregate.
/// </summary>
public interface IAiProviderRepository
{
    Task<AiProvider?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AiProvider?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<AiProvider>> ListAsync(CancellationToken ct = default);
    Task AddAsync(AiProvider provider, CancellationToken ct = default);
    void Update(AiProvider provider);
}
