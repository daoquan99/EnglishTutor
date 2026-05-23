using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Assessments.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Application.Abstractions;

namespace EnglishTutor.Modules.Users.Application.EventHandlers;

public sealed class LevelUpApprovedEventHandler(
    IUserTargetLanguageRepository targetLanguageRepository,
    IUsersInboxStore inboxStore,
    IUsersUnitOfWork unitOfWork)
    : IIntegrationEventHandler<LevelUpApprovedIntegrationEvent>
{
    private const string HandlerName = nameof(LevelUpApprovedEventHandler);

    public async Task HandleAsync(LevelUpApprovedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (!Enum.TryParse<LanguageLevel>(@event.NewLevel, true, out var newLevel))
        {
            throw new InvalidOperationException($"Unsupported level-up target level '{@event.NewLevel}'.");
        }

        var targetLanguages = await targetLanguageRepository.GetByUserIdAsync(@event.UserId, ct);
        var targetLanguage = targetLanguages.FirstOrDefault(language =>
            language.TargetLanguageCode.Value.Equals(@event.TargetLanguageCode, StringComparison.OrdinalIgnoreCase));
        if (targetLanguage is null)
        {
            throw new InvalidOperationException($"Target language '{@event.TargetLanguageCode}' was not found for user '{@event.UserId}'.");
        }

        targetLanguage.UpdateLevel(newLevel, @event.ApprovedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
