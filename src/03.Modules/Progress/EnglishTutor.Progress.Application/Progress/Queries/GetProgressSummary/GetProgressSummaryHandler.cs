using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress.Repositories;

namespace EnglishTutor.Progress.Application.Progress.Queries.GetProgressSummary;

internal sealed class GetProgressSummaryHandler
    : IQueryHandler<GetProgressSummaryQuery, IReadOnlyList<LearnerLanguageProgressView>>
{
    private readonly ILearnerLanguageProgressRepository _repository;

    public GetProgressSummaryHandler(ILearnerLanguageProgressRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<LearnerLanguageProgressView>>> Handle(
        GetProgressSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await _repository.ListByUserAsync(query.UserId, cancellationToken);
        var result = rows
            .Where(item => query.LanguagePairId == null || item.LanguagePairId == query.LanguagePairId)
            .Select(item => new LearnerLanguageProgressView(
                LanguagePairId: item.LanguagePairId,
                NativeLanguageCode: item.NativeLanguageCode,
                TargetLanguageCode: item.TargetLanguageCode,
                SessionsCompleted: item.SessionsCompleted,
                SpeakingSeconds: item.SpeakingSeconds,
                FeedbackCount: item.FeedbackCount,
                LatestScore: item.LatestScore,
                CurrentCefrLevel: item.CurrentCefrLevel,
                LastPracticedAtUtc: item.LastPracticedAtUtc))
            .ToArray();
        return Result.Success<IReadOnlyList<LearnerLanguageProgressView>>(result);
    }
}
