using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;

public sealed class GetTodayVocabularyQueryHandler(
    IUserVocabularyMasteryRepository masteryRepository,
    IVocabularyItemRepository vocabularyItemRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetTodayVocabularyQuery, TodayVocabularyResponse>
{
    public async Task<Result<TodayVocabularyResponse>> Handle(GetTodayVocabularyQuery request, CancellationToken cancellationToken)
    {
        var dueMasteries = await masteryRepository.GetDueAsync(
            request.UserId,
            request.TargetLanguageCode,
            dateTimeProvider.UtcNow,
            cancellationToken);
        var dueItemsById = (await vocabularyItemRepository.GetByIdsAsync(
            dueMasteries.Select(mastery => mastery.VocabularyItemId).Distinct().ToArray(),
            cancellationToken))
            .ToDictionary(item => item.Id);
        var dueItems = dueMasteries
            .Where(mastery => dueItemsById.ContainsKey(mastery.VocabularyItemId))
            .Select(mastery =>
            {
                var item = dueItemsById[mastery.VocabularyItemId];
                return new TodayVocabularyItemResponse(
                item.Id,
                item.Word,
                mastery.Status.ToString(),
                mastery.NextReviewAtUtc);
            })
            .ToList();
        var unseenItems = (await vocabularyItemRepository.GetNewItemsAsync(
                request.UserId,
                request.TargetLanguageCode,
                5,
                cancellationToken))
            .Select(item => new TodayVocabularyItemResponse(
                item.Id,
                item.Word,
                "New",
                dateTimeProvider.UtcNow))
            .ToList();

        return new TodayVocabularyResponse(dueItems.Concat(unseenItems).ToList());
    }
}
