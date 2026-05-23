using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.EventHandlers;

public sealed class UserRegisteredEventHandler(
    INotificationSettingRepository settingRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationsInboxStore inboxStore,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private const string HandlerName = nameof(UserRegisteredEventHandler);

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (await settingRepository.GetByUserIdAsync(@event.UserId, ct) is null)
        {
            var defaults = NotificationSettingsInitializer.CreateDefaults(
                @event.UserId, "UTC", dateTimeProvider.UtcNow);
            await settingRepository.AddAsync(defaults.Settings, ct);
            await scheduleRepository.AddRangeAsync(defaults.Schedules, ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
