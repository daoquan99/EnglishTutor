using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Contracts;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguagePairs;

internal sealed class ListLanguagePairsHandler
    : IQueryHandler<ListLanguagePairsQuery, IReadOnlyList<LearnerLanguagePairView>>
{
    private readonly ILearnerLanguagePortfolioRepository _portfolios;

    public ListLanguagePairsHandler(ILearnerLanguagePortfolioRepository portfolios)
    {
        _portfolios = portfolios;
    }

    public async Task<Result<IReadOnlyList<LearnerLanguagePairView>>> Handle(
        ListLanguagePairsQuery query,
        CancellationToken cancellationToken)
    {
        var portfolio = await _portfolios.GetByUserIdAsync(query.UserId, cancellationToken);
        var result = portfolio?.LanguagePairs
            .OrderByDescending(item => item.Status)
            .ThenBy(item => item.NativeLanguageCode)
            .ThenBy(item => item.TargetLanguageCode)
            .Select(item => new LearnerLanguagePairView(
                Id: item.Id,
                NativeLanguageCode: item.NativeLanguageCode,
                TargetLanguageCode: item.TargetLanguageCode,
                ExplanationLanguageCode: item.ExplanationLanguageCode,
                Status: item.Status.ToString(),
                Version: item.Version))
            .ToArray() ?? [];
        return Result.Success<IReadOnlyList<LearnerLanguagePairView>>(result);
    }
}
