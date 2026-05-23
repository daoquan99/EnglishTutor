using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class ExerciseCompletedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ExerciseCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(ExerciseCompletedEventHandler);

    public async Task HandleAsync(ExerciseCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var exp = 30 + (int)Math.Round(Math.Clamp(@event.Score, 0, 100) / 100m * 20m);
        var startedAt = @event.CompletedAtUtc.AddSeconds(-Math.Max(@event.TimeTakenSeconds, 0));

        await progressRepository.AddActivityLogAsync(LearningActivityLog.Create(
            @event.UserId,
            LanguageCode.Create(@event.TargetLanguageCode),
            ActivityType.ExerciseCompleted,
            @event.AttemptId,
            startedAt,
            @event.CompletedAtUtc,
            exp,
            @event.Score,
            "Completed"), ct);

        var experience = await progressRepository.GetOrCreateExperienceAsync(@event.UserId, @event.TargetLanguageCode, ct);
        experience.GrantExp(exp, nameof(ExerciseCompletedIntegrationEvent), @event.AttemptId, "Exercise completed", @event.CompletedAtUtc);

        foreach (var skill in ResolveSkills(@event.ExerciseType))
        {
            var progress = await progressRepository.GetOrCreateSkillProgressAsync(@event.UserId, @event.TargetLanguageCode, skill, ct);
            progress.RecordScore(@event.Score, @event.CompletedAtUtc);
        }

        await ProgressAggregationUpdater.RecordPeriodProgressAsync(progressRepository, @event.UserId, @event.TargetLanguageCode, @event.CompletedAtUtc, exp, ct);
        var streak = await progressRepository.GetOrCreateStreakAsync(@event.UserId, @event.TargetLanguageCode, ct);
        streak.RecordActivity(@event.CompletedAtUtc);
        var dashboard = await progressRepository.GetOrCreateDashboardSnapshotAsync(@event.UserId, @event.TargetLanguageCode, DateOnly.FromDateTime(@event.CompletedAtUtc), ct);
        dashboard.Update(
            experience.TotalExp,
            dashboard.CurrentLevel,
            streak.CurrentStreakDays,
            dashboard.VocabularyMastered,
            dashboard.TotalSpeakingSessions,
            dashboard.TotalExercisesCompleted + 1,
            dashboard.TotalMistakes,
            dashboard.WeakSkills,
            dashboard.StrongSkills,
            @event.CompletedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static IReadOnlyList<LearningSkill> ResolveSkills(string exerciseType) =>
        exerciseType switch
        {
            "MultipleChoice" or "FillInTheBlank" => [LearningSkill.Vocabulary, LearningSkill.Grammar],
            "VerbConjugation" or "SentenceCorrection" or "SentenceOrdering" => [LearningSkill.Grammar],
            "Translation" or "ShortWriting" => [LearningSkill.Writing, LearningSkill.Grammar],
            "ListeningChoice" => [LearningSkill.Listening],
            "ConversationCompletion" => [LearningSkill.Conversation],
            _ => [LearningSkill.Grammar]
        };
}
