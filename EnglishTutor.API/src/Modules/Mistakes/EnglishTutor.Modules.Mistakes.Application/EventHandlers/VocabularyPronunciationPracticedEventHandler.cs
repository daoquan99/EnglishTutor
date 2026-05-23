using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Mistakes.Application.EventHandlers;

public sealed class VocabularyPronunciationPracticedEventHandler(
    IMistakeRepository mistakeRepository,
    IUserLanguageSettingsReader userLanguageSettingsReader,
    IMistakesInboxStore inboxStore,
    IMistakesUnitOfWork unitOfWork)
    : IIntegrationEventHandler<VocabularyPronunciationPracticedIntegrationEvent>
{
    private const string HandlerName = nameof(VocabularyPronunciationPracticedEventHandler);

    public async Task HandleAsync(VocabularyPronunciationPracticedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (@event.PronunciationScore < 70)
        {
            var settings = await userLanguageSettingsReader.GetByUserIdAsync(@event.UserId, ct);
            await mistakeRepository.AddAsync(Mistake.CreateFromCorrection(
                @event.UserId,
                MistakeSourceType.VocabularyPronunciation,
                @event.VocabularyItemId,
                MistakeType.Pronunciation,
                "Pronunciation",
                "Vocabulary pronunciation",
                "Improve pronunciation accuracy",
                "Pronunciation score is below the review threshold.",
                LanguageCode.Create(@event.TargetLanguageCode),
                LanguageCode.Create(settings?.NativeLanguageCode ?? "en"),
                LanguageCode.Create(settings?.ExplanationLanguageCode ?? settings?.NativeLanguageCode ?? "en"),
                @event.PracticedAtUtc),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
