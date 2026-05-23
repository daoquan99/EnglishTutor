using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class SpeakingSessionCompletedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(SpeakingSessionCompletedEventHandler);

    public async Task HandleAsync(SpeakingSessionCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var exp = @event.OverallScore >= 80 ? 75 : 50;
        var completedAt = @event.CompletedAtUtc;
        var startedAt = completedAt.AddSeconds(-@event.DurationSeconds);
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.SpeakingSessionCompleted,
            @event.SessionId,
            startedAt,
            completedAt,
            exp,
            @event.OverallScore,
            "Completed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(SpeakingSessionCompletedIntegrationEvent), @event.SessionId, "Speaking session completed", completedAt);

        foreach (var skillKind in new[] { LearningSkill.Speaking, LearningSkill.Grammar, LearningSkill.Vocabulary, LearningSkill.Pronunciation })
        {
            var skill = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, skillKind, ct);
            skill.RecordScore(@event.OverallScore, completedAt);
        }

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(
            progressRepository,
            @event.UserId,
            @event.TargetLanguageCode,
            completedAt,
            exp,
            ct);

        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(completedAt);
        var dashboard = await progressRepository.GetOrCreateDashboardSnapshotAsync(@event.UserId, @event.TargetLanguageCode, DateOnly.FromDateTime(completedAt), ct);
        dashboard.Update(experience.TotalExp, dashboard.CurrentLevel, streak.CurrentStreakDays, dashboard.VocabularyMastered, dashboard.TotalSpeakingSessions + 1, dashboard.TotalExercisesCompleted, dashboard.TotalMistakes, dashboard.WeakSkills, "Speaking", completedAt);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
