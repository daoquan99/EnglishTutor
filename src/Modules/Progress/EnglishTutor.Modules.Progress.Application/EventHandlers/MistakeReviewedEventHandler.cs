using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class MistakeReviewedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<MistakeReviewedIntegrationEvent>
{
    private const string HandlerName = nameof(MistakeReviewedEventHandler);

    public async Task HandleAsync(MistakeReviewedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        const int exp = 10;
        var reviewedAt = @event.ReviewedAtUtc;
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.MistakeReviewed,
            @event.MistakeId,
            reviewedAt,
            reviewedAt,
            exp,
            100,
            "Reviewed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(MistakeReviewedIntegrationEvent), @event.MistakeId, "Mistake reviewed", reviewedAt);

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(
            progressRepository,
            @event.UserId,
            @event.TargetLanguageCode,
            reviewedAt,
            exp,
            ct);

        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(reviewedAt);
        var dashboard = await progressRepository.GetOrCreateDashboardSnapshotAsync(@event.UserId, @event.TargetLanguageCode, DateOnly.FromDateTime(reviewedAt), ct);
        dashboard.Update(
            experience.TotalExp,
            dashboard.CurrentLevel,
            streak.CurrentStreakDays,
            dashboard.VocabularyMastered,
            dashboard.TotalSpeakingSessions,
            dashboard.TotalExercisesCompleted,
            dashboard.TotalMistakes,
            dashboard.WeakSkills,
            dashboard.StrongSkills,
            reviewedAt);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
