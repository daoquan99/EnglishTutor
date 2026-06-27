using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Repositories;

public sealed class PracticeScenarioReadModelRepository : IPracticeScenarioReadModelRepository
{
    private readonly PracticeDbContext _context;

    public PracticeScenarioReadModelRepository(PracticeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PracticeScenarioReadModel readModel, CancellationToken ct)
    {
        await _context.ScenarioReadModels.AddAsync(readModel, ct);
    }

    public async Task<PracticeScenarioReadModel?> GetByIdAsync(Guid scenarioId, CancellationToken ct)
    {
        return await _context.ScenarioReadModels
            .FirstOrDefaultAsync(s => s.Id == scenarioId, ct);
    }

    public async Task UpdateAsync(PracticeScenarioReadModel readModel, CancellationToken ct)
    {
        _context.ScenarioReadModels.Update(readModel);
        await Task.CompletedTask;
    }
}
