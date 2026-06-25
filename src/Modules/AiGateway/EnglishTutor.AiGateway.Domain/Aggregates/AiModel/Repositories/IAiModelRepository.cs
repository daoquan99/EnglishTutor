using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;

/// <summary>
/// Repository interface for AiModel aggregate.
/// </summary>
public interface IAiModelRepository
{
    Task<AiModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AiModel?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(AiModel model, CancellationToken ct = default);
    void Update(AiModel model);
}
