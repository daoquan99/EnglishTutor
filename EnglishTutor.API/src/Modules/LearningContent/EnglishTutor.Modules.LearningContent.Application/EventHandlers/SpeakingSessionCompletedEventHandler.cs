using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.LearningContent.Application.EventHandlers;

public sealed class SpeakingSessionCompletedEventHandler(
    IConversationScenarioRepository scenarioRepository,
    ILearningPathCardRepository cardRepository,
    ILearningContentInboxStore inboxStore,
    ILearningContentUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(SpeakingSessionCompletedEventHandler);

    public async Task HandleAsync(SpeakingSessionCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (@event.ConversationScenarioId is not null)
        {
            var utcNow = dateTimeProvider.UtcNow;
            var card = await cardRepository.GetByContentAsync(
                @event.UserId,
                ContentType.ConversationScenario,
                @event.ConversationScenarioId.Value,
                ct);

            if (card is null)
            {
                var scenario = await scenarioRepository.GetByIdWithDetailsAsync(@event.ConversationScenarioId.Value, ct);
                if (scenario is not null)
                {
                    card = UserLearningPathCard.Create(
                        @event.UserId,
                        scenario.TargetLanguageCode,
                        ContentType.ConversationScenario,
                        scenario.Id,
                        scenario.Title,
                        scenario.Level.ToString(),
                        "Conversation",
                        scenario.Setting,
                        LearningPathCardStatus.Available,
                        scenario.EstimatedMinutes,
                        utcNow);
                    await cardRepository.AddAsync(card, ct);
                }
            }

            if (card is not null)
            {
                card.MarkCompleted((int)Math.Min(@event.DurationSeconds, int.MaxValue), utcNow);
                var nextCard = await cardRepository.GetNextAsync(@event.UserId, card.TargetLanguageCode, card.Order, ct);
                nextCard?.Unlock(utcNow);
            }
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
