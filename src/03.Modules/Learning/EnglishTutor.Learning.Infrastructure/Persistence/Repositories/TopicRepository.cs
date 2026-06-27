using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class TopicRepository : ITopicRepository
{
    private readonly LearningDbContext _db;

    public TopicRepository(LearningDbContext db)
    {
        _db = db;
    }

    public Task<Topic?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetByIdAsync(id, includeDeleted: false, ct);

    public async Task<Topic?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<Topic> query = _db.Topics.Include(t => t.TopicModes);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public Task<Topic?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        GetBySlugAsync(slug, includeDeleted: false, ct);

    public async Task<Topic?> GetBySlugAsync(string slug, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<Topic> query = _db.Topics.Include(t => t.TopicModes);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(t => t.Slug.Value == slug.ToLowerInvariant().Trim(), ct);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Topics
            .AsNoTracking()
            .AnyAsync(t => t.Slug.Value == slug.ToLowerInvariant().Trim(), ct);
    }

    public async Task<bool> ExistsActiveTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        return await _db.Topics
            .AsNoTracking()
            .AnyAsync(t => t.Id == topicId && t.IsActive, ct);
    }

    public async Task<IReadOnlyList<Topic>> ListActiveAsync(CancellationToken ct = default)
    {
        return await _db.Topics
            .Include(t => t.TopicModes)
            .Where(t => t.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Topic>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IQueryable<Topic> query = _db.Topics.Include(t => t.TopicModes);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(Topic topic, CancellationToken ct = default)
    {
        await _db.Topics.AddAsync(topic, ct);
    }

    public void AddTopicMode(TopicMode topicMode)
    {
        _db.TopicModes.Add(topicMode);
    }
}
