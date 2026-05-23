using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class VocabularyPronunciationPracticedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<VocabularyPronunciationPracticedIntegrationEvent>
{
    private const string HandlerName = nameof(VocabularyPronunciationPracticedEventHandler);

    public async Task HandleAsync(VocabularyPronunciationPracticedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var exp = @event.PronunciationScore >= 80 ? 15 : 5;
        var practicedAt = @event.PracticedAtUtc;
        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.VocabularyPronunciationPracticed,
            @event.VocabularyItemId,
            practicedAt,
            practicedAt,
            exp,
            @event.PronunciationScore,
            "Pronunciation practiced"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(VocabularyPronunciationPracticedIntegrationEvent), @event.VocabularyItemId, "Vocabulary pronunciation practiced", practicedAt);

        var skill = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, LearningSkill.Pronunciation, ct);
        skill.RecordScore(@event.PronunciationScore, practicedAt);

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(
            progressRepository,
            @event.UserId,
            @event.TargetLanguageCode,
            practicedAt,
            exp,
            ct);

        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(practicedAt);

        var dashboard = await progressRepository.GetOrCreateDashboardSnapshotAsync(@event.UserId, @event.TargetLanguageCode, DateOnly.FromDateTime(practicedAt), ct);
        dashboard.Update(experience.TotalExp, dashboard.CurrentLevel, streak.CurrentStreakDays, dashboard.VocabularyMastered, dashboard.TotalSpeakingSessions, dashboard.TotalExercisesCompleted, dashboard.TotalMistakes, dashboard.WeakSkills, "Pronunciation", practicedAt);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
