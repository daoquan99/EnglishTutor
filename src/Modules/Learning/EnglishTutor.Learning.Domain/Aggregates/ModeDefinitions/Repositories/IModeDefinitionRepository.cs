using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;

public interface IModeDefinitionRepository
{
    Task<ModeDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ModeDefinition?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default);
    Task<ModeDefinition?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<ModeDefinition?> GetByCodeAsync(string code, bool includeDeleted, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ModeDefinition>> ListActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ModeDefinition>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default);
    Task AddAsync(ModeDefinition modeDefinition, CancellationToken ct = default);
}
