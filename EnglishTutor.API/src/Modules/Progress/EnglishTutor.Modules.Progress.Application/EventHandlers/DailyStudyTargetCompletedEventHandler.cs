using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class DailyStudyTargetCompletedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<DailyStudyTargetCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(DailyStudyTargetCompletedEventHandler);

    public async Task HandleAsync(DailyStudyTargetCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        const int exp = 20;
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.DailyGoalCompleted,
            @event.EventId,
            @event.CompletedDateUtc,
            @event.CompletedDateUtc,
            exp,
            100,
            "Completed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(DailyStudyTargetCompletedIntegrationEvent), @event.EventId, "Daily study target completed", @event.CompletedDateUtc);
        await ProgressAggregationUpdater.RecordPeriodProgressAsync(progressRepository, @event.UserId, @event.TargetLanguageCode, @event.CompletedDateUtc, exp, ct);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
