using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Learning;

/// <summary>
/// Note (CQRS pragmatic side-effect): Although served via HTTP GET (/api/learning/flashcards/today),
/// this operation ensures the daily learning plan exists, carries over pending past items, and schedules
/// reviews. It delegates to InitializeDailyVocabularyPlanCommandHandler.
/// </summary>
public sealed record GetDailyVocabularyPlanQuery(
    string UserId,
    string? TimeZoneId,
    int? Target
) : IQuery<DailyVocabularyPlanResponseDto>;

public sealed class GetDailyVocabularyPlanQueryHandler : IQueryHandler<GetDailyVocabularyPlanQuery, DailyVocabularyPlanResponseDto>
{
    private readonly ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto> _initHandler;

    public GetDailyVocabularyPlanQueryHandler(
        ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto> initHandler)
    {
        _initHandler = initHandler;
    }

    public async Task<DailyVocabularyPlanResponseDto> HandleAsync(GetDailyVocabularyPlanQuery query, CancellationToken ct = default)
    {
        return await _initHandler.HandleAsync(
            new InitializeDailyVocabularyPlanCommand(
                query.UserId,
                new InitializeDailyPlanRequest(
                    ApiKey: null,
                    NewVocabularyTarget: query.Target ?? 20,
                    TimeZoneId: query.TimeZoneId ?? "Asia/Ho_Chi_Minh",
                    GenerateWithAiIfNeeded: false
                )
            ),
            ct);
    }
}

public sealed record GetLearningProgressQuery(string UserId) : IQuery<object>;

public sealed class GetLearningProgressQueryHandler : IQueryHandler<GetLearningProgressQuery, object>
{
    private readonly IAppDbContextFactory _dbFactory;

    public GetLearningProgressQueryHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<object> HandleAsync(GetLearningProgressQuery query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var userId = query.UserId;

        var progresses = await db.UserVocabularyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        int totalCatalogSenses = await db.VocabularySenses.CountAsync(ct);
        int totalAssessments = await db.PronunciationAssessments.CountAsync(a => a.UserId == userId, ct);
        decimal? avgPronunciation = await db.PronunciationAssessments
            .Where(a => a.UserId == userId && a.OverallScore != null)
            .AverageAsync(a => a.OverallScore, ct);

        return new
        {
            userId,
            totalCatalogSenses,
            totalStudied = progresses.Count,
            masteredCount = progresses.Count(p => p.LearningStatus == "Mastered"),
            reviewCount = progresses.Count(p => p.LearningStatus == "Review"),
            learningCount = progresses.Count(p => p.LearningStatus == "Learning"),
            dueForReviewNow = progresses.Count(p => p.NextReviewAt != null && p.NextReviewAt <= now),
            averageMastery = progresses.Count == 0 ? 0 : Math.Round(progresses.Average(p => p.MasteryLevel), 1),
            totalPronunciationAssessments = totalAssessments,
            averagePronunciationScore = avgPronunciation.HasValue ? Math.Round(avgPronunciation.Value, 1) : (decimal?)null
        };
    }
}

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
