using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;

public interface IPracticeScenarioReadModelRepository
{
    Task AddAsync(PracticeScenarioReadModel readModel, CancellationToken ct);
    Task<PracticeScenarioReadModel?> GetByIdAsync(Guid scenarioId, CancellationToken ct);
    Task UpdateAsync(PracticeScenarioReadModel readModel, CancellationToken ct);
}
