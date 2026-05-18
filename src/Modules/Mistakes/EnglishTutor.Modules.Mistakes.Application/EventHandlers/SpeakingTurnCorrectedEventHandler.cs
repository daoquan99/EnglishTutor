using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Mistakes.Application.EventHandlers;

public sealed class SpeakingTurnCorrectedEventHandler(
    IMistakeRepository mistakeRepository,
    IMistakesInboxStore inboxStore,
    IMistakesUnitOfWork unitOfWork)
    : IIntegrationEventHandler<SpeakingTurnCorrectedIntegrationEvent>
{
    private const string HandlerName = nameof(SpeakingTurnCorrectedEventHandler);

    public async Task HandleAsync(SpeakingTurnCorrectedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        foreach (var mistakeDetail in @event.Mistakes)
        {
            var type = Enum.TryParse<MistakeType>(mistakeDetail.Type, true, out var parsedType)
                ? parsedType
                : MistakeType.Grammar;

            await mistakeRepository.AddAsync(Mistake.CreateFromCorrection(
                @event.UserId,
                MistakeSourceType.SpeakingTurn,
                @event.TurnId,
                type,
                mistakeDetail.Type,
                mistakeDetail.Original,
                mistakeDetail.Corrected,
                mistakeDetail.Explanation,
                LanguageCode.Create(@event.TargetLanguageCode),
                LanguageCode.Create(string.IsNullOrWhiteSpace(@event.NativeLanguageCode) ? "vi" : @event.NativeLanguageCode),
                LanguageCode.Create(string.IsNullOrWhiteSpace(@event.ExplanationLanguageCode) ? "vi" : @event.ExplanationLanguageCode)),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
