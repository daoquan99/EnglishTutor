using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.EventHandlers;

public sealed class UserRegisteredIntegrationEventHandler(
    IUserProfileRepository userProfileRepository,
    IUserLanguageSettingsRepository userLanguageSettingsRepository,
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IUsersInboxStore inboxStore,
    IUsersUnitOfWork unitOfWork)
    : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private const string HandlerName = nameof(UserRegisteredIntegrationEventHandler);

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (await userProfileRepository.GetByUserIdAsync(@event.UserId, ct) is null)
        {
            var utcNow = @event.OccurredOnUtc;
            var profile = UserProfile.Create(@event.UserId, @event.DisplayName, utcNow);
            var settings = UserLanguageSettings.CreateDefault(@event.UserId, utcNow);
            var targetLanguage = UserTargetLanguage.CreateActive(
                @event.UserId,
                BuildingBlocks.SharedKernel.LanguageCode.English,
                BuildingBlocks.SharedKernel.LanguageLevel.A1,
                BuildingBlocks.SharedKernel.LanguageLevel.B2,
                [],
                utcNow);

            await userProfileRepository.AddAsync(profile, ct);
            await userLanguageSettingsRepository.AddAsync(settings, ct);
            await userTargetLanguageRepository.AddAsync(targetLanguage, ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
