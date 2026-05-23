using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class VocabularyReviewedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<VocabularyReviewedIntegrationEvent>
{
    private const string HandlerName = nameof(VocabularyReviewedEventHandler);

    public async Task HandleAsync(VocabularyReviewedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var exp = @event.IsCorrect ? 20 : 10;
        var completedAt = @event.ReviewedAtUtc;
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.VocabularyReviewed,
            @event.VocabularyItemId,
            completedAt,
            completedAt,
            exp,
            @event.Score,
            @event.MasteryStatus), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(VocabularyReviewedIntegrationEvent), @event.VocabularyItemId, "Vocabulary review", completedAt);

        var skill = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, LearningSkill.Vocabulary, ct);
        skill.RecordScore(@event.Score, completedAt);

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(
            progressRepository,
            @event.UserId,
            @event.TargetLanguageCode,
            @event.ReviewedAtUtc,
            exp,
            ct);

        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(@event.ReviewedAtUtc);

        var dashboard = await progressRepository.GetOrCreateDashboardSnapshotAsync(@event.UserId, @event.TargetLanguageCode, DateOnly.FromDateTime(@event.ReviewedAtUtc), ct);
        dashboard.Update(experience.TotalExp, dashboard.CurrentLevel, streak.CurrentStreakDays, dashboard.VocabularyMastered, dashboard.TotalSpeakingSessions, dashboard.TotalExercisesCompleted, dashboard.TotalMistakes, dashboard.WeakSkills, "Vocabulary", completedAt);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
