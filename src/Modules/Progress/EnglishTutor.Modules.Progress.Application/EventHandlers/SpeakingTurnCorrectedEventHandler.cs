using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Progress.Application.EventHandlers;

public sealed class SpeakingTurnCorrectedEventHandler(
    IProgressRepository progressRepository,
    IProgressInboxStore inboxStore,
    IProgressUnitOfWork unitOfWork)
    : IIntegrationEventHandler<SpeakingTurnCorrectedIntegrationEvent>
{
    private const string HandlerName = nameof(SpeakingTurnCorrectedEventHandler);

    public async Task HandleAsync(SpeakingTurnCorrectedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var speaking = await progressRepository.GetOrCreateSkillProgressAsync(
            @event.UserId,
            @event.TargetLanguageCode,
            LearningSkill.Speaking,
            ct);
        speaking.RecordScore(@event.OverallScore);

        var grammar = await progressRepository.GetOrCreateSkillProgressAsync(
            @event.UserId,
            @event.TargetLanguageCode,
            LearningSkill.Grammar,
            ct);
        grammar.RecordScore(@event.GrammarScore);

        var vocabulary = await progressRepository.GetOrCreateSkillProgressAsync(
            @event.UserId,
            @event.TargetLanguageCode,
            LearningSkill.Vocabulary,
            ct);
        vocabulary.RecordScore(@event.VocabularyScore);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
