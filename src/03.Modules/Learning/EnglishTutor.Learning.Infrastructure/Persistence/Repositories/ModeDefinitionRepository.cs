using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class ModeDefinitionRepository : IModeDefinitionRepository
{
    private readonly LearningDbContext _db;

    public ModeDefinitionRepository(LearningDbContext db)
    {
        _db = db;
    }

    public Task<ModeDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetByIdAsync(id, includeDeleted: false, ct);

    public async Task<ModeDefinition?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<ModeDefinition> query = _db.ModeDefinitions;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public Task<ModeDefinition?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        GetByCodeAsync(code, includeDeleted: false, ct);

    public async Task<ModeDefinition?> GetByCodeAsync(string code, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<ModeDefinition> query = _db.ModeDefinitions;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(m => m.Code.Value == code.ToLowerInvariant().Trim(), ct);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _db.ModeDefinitions
            .AsNoTracking()
            .AnyAsync(m => m.Code.Value == code.ToLowerInvariant().Trim(), ct);
    }

    public async Task<IReadOnlyList<ModeDefinition>> ListActiveAsync(CancellationToken ct = default)
    {
        return await _db.ModeDefinitions
            .Where(m => m.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ModeDefinition>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IQueryable<ModeDefinition> query = _db.ModeDefinitions;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(ModeDefinition modeDefinition, CancellationToken ct = default)
    {
        await _db.ModeDefinitions.AddAsync(modeDefinition, ct);
    }
}
