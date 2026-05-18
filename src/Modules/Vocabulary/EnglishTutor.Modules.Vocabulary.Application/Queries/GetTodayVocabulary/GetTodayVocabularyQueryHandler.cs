using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;

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

        var allItems = await vocabularyItemRepository.GetByTargetLanguageAsync(request.TargetLanguageCode, cancellationToken);
        var dueItems = dueMasteries.Select(mastery =>
        {
            var item = allItems.Single(vocabulary => vocabulary.Id == mastery.VocabularyItemId);
            return new TodayVocabularyItemResponse(
                item.Id,
                item.Word,
                mastery.Status.ToString(),
                mastery.NextReviewAtUtc);
        }).ToList();

        var unseenItems = allItems
            .Where(item => dueMasteries.All(mastery => mastery.VocabularyItemId != item.Id))
            .Take(5)
            .Select(item => new TodayVocabularyItemResponse(
                item.Id,
                item.Word,
                "New",
                dateTimeProvider.UtcNow))
            .ToList();

        return new TodayVocabularyResponse(dueItems.Concat(unseenItems).ToList());
    }
}
