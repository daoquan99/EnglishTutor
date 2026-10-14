using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Learning;

/// <summary>
/// Note (CQRS pragmatic side-effect): Although served via HTTP GET (/api/learning/flashcards/today),
/// this operation ensures the daily learning plan exists, carries over pending past items, and schedules
/// reviews. It delegates to InitializeDailyVocabularyPlanCommandHandler.
/// </summary>

public sealed record GetLearningHistoryQuery(string UserId, int Limit = 30) : IQuery<object>;

public sealed class GetLearningHistoryQueryHandler : IQueryHandler<GetLearningHistoryQuery, object>
{
    private readonly IAppDbContextFactory _dbFactory;

    public GetLearningHistoryQueryHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<object> HandleAsync(GetLearningHistoryQuery query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var recentReviews = await db.VocabularyReviewAttempts
            .AsNoTracking()
            .Where(r => r.UserId == query.UserId)
            .OrderByDescending(r => r.ReviewedAt)
            .Take(query.Limit)
            .ToListAsync(ct);

        var vocabIds = recentReviews.Select(r => r.VocabularyId).Distinct().ToList();
        var vocabDict = await db.Vocabularies
            .AsNoTracking()
            .Where(v => vocabIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Word, ct);

        return recentReviews.Select(r => new
        {
            r.Id,
            r.VocabularyId,
            r.SenseId,
            Word = vocabDict.TryGetValue(r.VocabularyId, out var w) ? w : "Vocabulary",
            r.ReviewType,
            r.Rating,
            r.ReviewedAt,
            r.NextReviewAtSnapshot
        });
    }
}
