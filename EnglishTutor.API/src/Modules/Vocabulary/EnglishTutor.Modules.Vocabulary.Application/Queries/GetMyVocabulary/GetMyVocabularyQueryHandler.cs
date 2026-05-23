using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetMyVocabulary;

public sealed class GetMyVocabularyQueryHandler(
    IUserVocabularyMasteryRepository masteryRepository,
    IVocabularyItemRepository vocabularyItemRepository)
    : IQueryHandler<GetMyVocabularyQuery, MyVocabularyResponse>
{
    public async Task<Result<MyVocabularyResponse>> Handle(GetMyVocabularyQuery request, CancellationToken cancellationToken)
    {
        var masteries = await masteryRepository.GetByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);

        if (masteries.Count == 0)
        {
            return new MyVocabularyResponse([], new MyVocabularyCountsResponse(0, 0, 0, 0, 0, 0));
        }

        var itemIds = masteries.Select(m => m.VocabularyItemId).Distinct().ToArray();
        var itemsById = (await vocabularyItemRepository.GetByIdsAsync(itemIds, cancellationToken))
            .ToDictionary(item => item.Id);

        var items = masteries
            .Where(m => itemsById.ContainsKey(m.VocabularyItemId))
            .Select(m =>
            {
                var item = itemsById[m.VocabularyItemId];
                return new MyVocabularyItemResponse(
                    item.Id,
                    item.Word,
                    item.Phonetic,
                    m.Status.ToString(),
                    m.MeaningMasteryScore,
                    m.ReviewCount,
                    m.NextReviewAtUtc);
            })
            .ToList();

        var counts = new MyVocabularyCountsResponse(
            masteries.Count(m => m.Status == VocabularyMasteryStatus.New),
            masteries.Count(m => m.Status == VocabularyMasteryStatus.Learning),
            masteries.Count(m => m.Status == VocabularyMasteryStatus.Reviewing),
            masteries.Count(m => m.Status == VocabularyMasteryStatus.Weak),
            masteries.Count(m => m.Status == VocabularyMasteryStatus.Mastered),
            masteries.Count);

        return new MyVocabularyResponse(items, counts);
    }
}
