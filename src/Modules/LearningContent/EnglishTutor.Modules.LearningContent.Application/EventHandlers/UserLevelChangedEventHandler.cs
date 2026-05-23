using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.LearningContent.Application.EventHandlers;

public sealed class UserLevelChangedEventHandler(
    ILessonRepository lessonRepository,
    IConversationScenarioRepository scenarioRepository,
    ILearningPathCardRepository cardRepository,
    ILearningContentInboxStore inboxStore,
    ILearningContentUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<UserLevelChangedIntegrationEvent>
{
    private const string HandlerName = nameof(UserLevelChangedEventHandler);

    public async Task HandleAsync(UserLevelChangedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var utcNow = dateTimeProvider.UtcNow;
        var cards = (await cardRepository.ListAsync(@event.UserId, @event.TargetLanguageCode, ct)).ToList();
        var nextOrder = cards.Count == 0 ? 1 : cards.Max(card => card.Order) + 1;

        var lessons = await lessonRepository.ListPublishedAsync(1, 100, @event.NewLevel, null, null, @event.TargetLanguageCode, ct);
        foreach (var lesson in lessons)
        {
            if (cards.Any(card => card.ContentType == ContentType.Lesson && card.ContentId == lesson.Id))
            {
                continue;
            }

            var card = UserLearningPathCard.Create(
                @event.UserId,
                lesson.TargetLanguageCode,
                ContentType.Lesson,
                lesson.Id,
                lesson.Title,
                lesson.Level.ToString(),
                lesson.Skill.ToString(),
                lesson.Topic,
                cards.Count == 0 && nextOrder == 1 ? LearningPathCardStatus.Available : LearningPathCardStatus.Locked,
                nextOrder++,
                utcNow);
            await cardRepository.AddAsync(card, ct);
            cards.Add(card);
        }

        var scenarios = await scenarioRepository.ListPublishedAsync(1, 100, @event.NewLevel, null, @event.TargetLanguageCode, ct);
        foreach (var scenario in scenarios)
        {
            if (cards.Any(card => card.ContentType == ContentType.ConversationScenario && card.ContentId == scenario.Id))
            {
                continue;
            }

            var card = UserLearningPathCard.Create(
                @event.UserId,
                scenario.TargetLanguageCode,
                ContentType.ConversationScenario,
                scenario.Id,
                scenario.Title,
                scenario.Level.ToString(),
                "Conversation",
                scenario.Setting,
                cards.Count == 0 && nextOrder == 1 ? LearningPathCardStatus.Available : LearningPathCardStatus.Locked,
                nextOrder++,
                utcNow);
            await cardRepository.AddAsync(card, ct);
            cards.Add(card);
        }

        foreach (var card in cards.Where(card => IsAboveLevel(card.Level, @event.NewLevel)))
        {
            card.Lock(utcNow);
        }

        if (!cards.Any(card => card.Status == LearningPathCardStatus.Available || card.Status == LearningPathCardStatus.InProgress))
        {
            cards.Where(card => !IsAboveLevel(card.Level, @event.NewLevel))
                .OrderBy(card => card.Order)
                .FirstOrDefault()
                ?.Unlock(utcNow);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static bool IsAboveLevel(string candidateLevel, string currentLevel)
    {
        if (!Enum.TryParse<LanguageLevel>(candidateLevel, true, out var candidate) ||
            !Enum.TryParse<LanguageLevel>(currentLevel, true, out var current))
        {
            return false;
        }

        return candidate > current;
    }
}
