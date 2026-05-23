using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class ConversationScenarioCompletedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ConversationScenarioCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(ConversationScenarioCompletedEventHandler);

    public async Task HandleAsync(ConversationScenarioCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        const int exp = 60;
        var startedAt = @event.CompletedAtUtc.AddSeconds(-Math.Max(@event.DurationSeconds, 0));
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.SpeakingSessionCompleted,
            @event.ConversationScenarioId,
            startedAt,
            @event.CompletedAtUtc,
            exp,
            100,
            "Completed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(ConversationScenarioCompletedIntegrationEvent), @event.ConversationScenarioId, "Conversation scenario completed", @event.CompletedAtUtc);
        var speaking = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, LearningSkill.Conversation, ct);
        speaking.RecordScore(100, @event.CompletedAtUtc);
        await ProgressAggregationUpdater.RecordPeriodProgressAsync(progressRepository, @event.UserId, @event.TargetLanguageCode, @event.CompletedAtUtc, exp, ct);
        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(@event.CompletedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
