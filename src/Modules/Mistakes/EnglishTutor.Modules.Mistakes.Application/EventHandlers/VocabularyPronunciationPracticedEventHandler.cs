using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Mistakes.Application.EventHandlers;

public sealed class VocabularyPronunciationPracticedEventHandler(
    IMistakeRepository mistakeRepository,
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
                LanguageCode.Vietnamese,
                LanguageCode.Vietnamese),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
