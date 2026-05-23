using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;

public sealed class GetTodayVocabularyQueryHandler(
    IUserVocabularyMasteryRepository masteryRepository,
    IVocabularyItemRepository vocabularyItemRepository,
    IVocabularyStudySettingsRepository settingsRepository,
    IDateTimeProvider dateTimeProvider,
    IVocabularyUnitOfWork unitOfWork)
    : IQueryHandler<GetTodayVocabularyQuery, TodayVocabularyResponse>
{
    public async Task<Result<TodayVocabularyResponse>> Handle(GetTodayVocabularyQuery request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var todayStartUtc = utcNow.Date;

        var settings = await settingsRepository.GetAsync(request.UserId, request.TargetLanguageCode, cancellationToken);
        if (settings is null)
        {
            settings = VocabularyStudySettings.CreateDefault(
                request.UserId,
                LanguageCode.Create(request.TargetLanguageCode),
                utcNow);
            await settingsRepository.AddAsync(settings, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var dueMasteries = await masteryRepository.GetDueAsync(
            request.UserId,
            request.TargetLanguageCode,
            utcNow,
            cancellationToken);

        var reviewMasteries = dueMasteries
            .Where(m => settings.IncludeMasteredInReview || m.Status != VocabularyMasteryStatus.Mastered)
            .Take(settings.ReviewWordsPerDay)
            .ToList();

        var todayAssignedCount = await masteryRepository.GetTodayAssignedCountAsync(
            request.UserId,
            request.TargetLanguageCode,
            todayStartUtc,
            cancellationToken);

        var remaining = settings.NewWordsPerDay - todayAssignedCount;
        if (remaining > 0)
        {
            var newItems = await vocabularyItemRepository.GetNewItemsAsync(
                request.UserId,
                request.TargetLanguageCode,
                remaining,
                cancellationToken);

            foreach (var item in newItems)
            {
                var mastery = UserVocabularyMastery.Create(
                    request.UserId,
                    item.Id,
                    LanguageCode.Create(request.TargetLanguageCode),
                    utcNow);
                await masteryRepository.AddAsync(mastery, cancellationToken);
            }

            if (newItems.Count > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var todayNewMasteries = await masteryRepository.GetByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);

        var newMasteries = todayNewMasteries
            .Where(m => m.CreatedAtUtc >= todayStartUtc && m.Status == VocabularyMasteryStatus.New)
            .ToList();

        var allItemIds = reviewMasteries.Select(m => m.VocabularyItemId)
            .Concat(newMasteries.Select(m => m.VocabularyItemId))
            .Distinct()
            .ToArray();

        var itemsById = (await vocabularyItemRepository.GetByIdsAsync(allItemIds, cancellationToken))
            .ToDictionary(item => item.Id);

        var reviewItems = reviewMasteries
            .Where(m => itemsById.ContainsKey(m.VocabularyItemId))
            .Select(m => MapToResponse(itemsById[m.VocabularyItemId], m))
            .ToList();

        var newWordItems = newMasteries
            .Where(m => itemsById.ContainsKey(m.VocabularyItemId))
            .Select(m => MapToResponse(itemsById[m.VocabularyItemId], m))
            .ToList();

        return new TodayVocabularyResponse(
            newWordItems,
            reviewItems,
            newWordItems.Count,
            reviewItems.Count,
            settings.NewWordsPerDay);
    }

    private static TodayVocabularyItemResponse MapToResponse(VocabularyItem item, UserVocabularyMastery mastery) =>
        new(item.Id,
            item.Word,
            item.Phonetic,
            mastery.Status.ToString(),
            mastery.MeaningMasteryScore,
            mastery.ReviewCount,
            mastery.NextReviewAtUtc);
}
