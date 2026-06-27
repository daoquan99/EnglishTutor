using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class TopicPhraseRepository : ITopicPhraseRepository
{
    private readonly LearningDbContext _db;

    public TopicPhraseRepository(LearningDbContext db)
    {
        _db = db;
    }

    public async Task<TopicPhrase?> GetByIdForTopicAsync(Guid topicId, Guid phraseId, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicPhrase> query = _db.TopicPhrases;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(p => p.TopicId == topicId && p.Id == phraseId, ct);
    }

    public async Task<bool> ExistsByPhraseAsync(Guid topicId, string normalizedPhrase, Guid? excludePhraseId, CancellationToken ct = default)
    {
        IQueryable<TopicPhrase> query = _db.TopicPhrases.AsNoTracking();
        if (excludePhraseId.HasValue)
        {
            query = query.Where(p => p.Id != excludePhraseId.Value);
        }
        return await query.AnyAsync(p => p.TopicId == topicId && p.PhraseNormalized == normalizedPhrase, ct);
    }

    public async Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _db.TopicPhrases
            .AsNoTracking()
            .Where(p => p.TopicId == topicId && p.IsActive)
            .OrderBy(p => p.Phrase)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicPhrase> query = _db.TopicPhrases.AsNoTracking();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(p => p.TopicId == topicId);

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderBy(p => p.Phrase)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicPhrase> query = _db.TopicPhrases.AsNoTracking();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(p => p.TopicId == topicId);

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.CountAsync(ct);
    }

    public void Add(TopicPhrase phrase)
    {
        _db.TopicPhrases.Add(phrase);
    }
}
