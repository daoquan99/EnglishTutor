using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class ScenarioRepository : IScenarioRepository
{
    private readonly LearningDbContext _db;

    public ScenarioRepository(LearningDbContext db)
    {
        _db = db;
    }

    public Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetByIdAsync(id, includeDeleted: false, ct);

    public async Task<Scenario?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<Scenario> query = _db.Scenarios;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(Guid topicId, Guid modeDefinitionId, string name, CancellationToken ct = default)
    {
        return await _db.Scenarios
            .AsNoTracking()
            .AnyAsync(s => s.TopicId == topicId && 
                           s.ModeDefinitionId == modeDefinitionId && 
                           s.Name.ToLower() == name.ToLower().Trim(), ct);
    }

    public async Task<IReadOnlyList<Scenario>> ListActiveForTopicModeAsync(Guid topicId, Guid modeDefinitionId, CancellationToken ct = default)
    {
        return await _db.Scenarios
            .Where(s => s.TopicId == topicId && 
                        s.ModeDefinitionId == modeDefinitionId && 
                        s.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Scenario>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IQueryable<Scenario> query = _db.Scenarios;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(Scenario scenario, CancellationToken ct = default)
    {
        await _db.Scenarios.AddAsync(scenario, ct);
    }
}
