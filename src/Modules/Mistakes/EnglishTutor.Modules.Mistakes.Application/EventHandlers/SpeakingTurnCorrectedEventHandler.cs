using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Mistakes.Application.EventHandlers;

public sealed class SpeakingTurnCorrectedEventHandler(
    IMistakeRepository mistakeRepository,
    IUserLanguageSettingsReader userLanguageSettingsReader,
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

        string nativeLanguageCode = @event.NativeLanguageCode;
        string explanationLanguageCode = @event.ExplanationLanguageCode;

        if (string.IsNullOrWhiteSpace(nativeLanguageCode) || string.IsNullOrWhiteSpace(explanationLanguageCode))
        {
            var settings = await userLanguageSettingsReader.GetByUserIdAsync(@event.UserId, ct);
            if (string.IsNullOrWhiteSpace(nativeLanguageCode))
            {
                nativeLanguageCode = !string.IsNullOrWhiteSpace(settings?.NativeLanguageCode) ? settings.NativeLanguageCode : "en";
            }
            if (string.IsNullOrWhiteSpace(explanationLanguageCode))
            {
                explanationLanguageCode = !string.IsNullOrWhiteSpace(settings?.ExplanationLanguageCode) 
                    ? settings.ExplanationLanguageCode 
                    : (!string.IsNullOrWhiteSpace(settings?.NativeLanguageCode) ? settings.NativeLanguageCode : "en");
            }
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
                LanguageCode.Create(nativeLanguageCode),
                LanguageCode.Create(explanationLanguageCode),
                @event.CorrectedAtUtc),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
