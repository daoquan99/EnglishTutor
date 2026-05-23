using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class LessonCompletedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<LessonCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(LessonCompletedEventHandler);

    public async Task HandleAsync(LessonCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        const int exp = 40;
        var startedAt = @event.CompletedAtUtc.AddSeconds(-Math.Max(@event.DurationSeconds, 0));
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.LessonCompleted,
            @event.LessonId,
            startedAt,
            @event.CompletedAtUtc,
            exp,
            100,
            "Completed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(LessonCompletedIntegrationEvent), @event.LessonId, "Lesson completed", @event.CompletedAtUtc);

        if (Enum.TryParse<LearningSkill>(@event.Skill, true, out var skillKind))
        {
            var skill = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, skillKind, ct);
            skill.RecordScore(100, @event.CompletedAtUtc);
        }

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(progressRepository, @event.UserId, @event.TargetLanguageCode, @event.CompletedAtUtc, exp, ct);
        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(@event.CompletedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
