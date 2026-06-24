using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;

public interface IScenarioRepository
{
    Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Scenario?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid topicId, Guid modeDefinitionId, string name, CancellationToken ct = default);
    Task<IReadOnlyList<Scenario>> ListActiveForTopicModeAsync(Guid topicId, Guid modeDefinitionId, CancellationToken ct = default);
    Task<IReadOnlyList<Scenario>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default);
    Task AddAsync(Scenario scenario, CancellationToken ct = default);
}
