using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class TopicVocabularyRepository : ITopicVocabularyRepository
{
    private readonly LearningDbContext _db;

    public TopicVocabularyRepository(LearningDbContext db)
    {
        _db = db;
    }

    public async Task<TopicVocabulary?> GetByIdForTopicAsync(Guid topicId, Guid vocabularyId, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicVocabulary> query = _db.TopicVocabularies;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(v => v.TopicId == topicId && v.Id == vocabularyId, ct);
    }

    public async Task<bool> ExistsByWordAsync(Guid topicId, string normalizedWord, Guid? excludeVocabularyId, CancellationToken ct = default)
    {
        IQueryable<TopicVocabulary> query = _db.TopicVocabularies.AsNoTracking();
        if (excludeVocabularyId.HasValue)
        {
            query = query.Where(v => v.Id != excludeVocabularyId.Value);
        }
        return await query.AnyAsync(v => v.TopicId == topicId && v.WordNormalized == normalizedWord, ct);
    }

    public async Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _db.TopicVocabularies
            .AsNoTracking()
            .Where(v => v.TopicId == topicId && v.IsActive)
            .OrderBy(v => v.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicVocabulary> query = _db.TopicVocabularies.AsNoTracking();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(v => v.TopicId == topicId);

        if (!includeInactive)
        {
            query = query.Where(v => v.IsActive);
        }

        return await query
            .OrderBy(v => v.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default)
    {
        IQueryable<TopicVocabulary> query = _db.TopicVocabularies.AsNoTracking();
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(v => v.TopicId == topicId);

        if (activeOnly)
        {
            query = query.Where(v => v.IsActive);
        }

        return await query.CountAsync(ct);
    }

    public void Add(TopicVocabulary vocabulary)
    {
        _db.TopicVocabularies.Add(vocabulary);
    }
}
