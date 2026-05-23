using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class UserLevelChangedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<UserLevelChangedIntegrationEvent>
{
    private const string HandlerName = nameof(UserLevelChangedEventHandler);
    private const int LevelUpExp = 500;

    public async Task HandleAsync(UserLevelChangedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.LevelUp,
            @event.EventId,
            @event.ChangedAtUtc,
            @event.ChangedAtUtc,
            LevelUpExp,
            100,
            $"{@event.PreviousLevel}->{@event.NewLevel}"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(LevelUpExp, nameof(UserLevelChangedIntegrationEvent), @event.EventId, "Level up approved", @event.ChangedAtUtc);

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(progressRepository, @event.UserId, @event.TargetLanguageCode, @event.ChangedAtUtc, LevelUpExp, ct);

        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(@event.ChangedAtUtc);

        var snapshot = await progressRepository.GetOrCreateDashboardSnapshotAsync(
            @event.UserId,
            @event.TargetLanguageCode,
            DateOnly.FromDateTime(@event.ChangedAtUtc),
            ct);
        snapshot.Update(
            experience.TotalExp,
            @event.NewLevel,
            streak.CurrentStreakDays,
            snapshot.VocabularyMastered,
            snapshot.TotalSpeakingSessions,
            snapshot.TotalExercisesCompleted,
            snapshot.TotalMistakes,
            snapshot.WeakSkills,
            snapshot.StrongSkills,
            @event.ChangedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
