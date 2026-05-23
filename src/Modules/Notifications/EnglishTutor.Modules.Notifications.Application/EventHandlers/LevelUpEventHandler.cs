using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.Notifications.Application.EventHandlers;

public sealed class LevelUpEventHandler(
    INotificationRepository notificationRepository,
    INotificationsInboxStore inboxStore,
    INotificationsUnitOfWork unitOfWork,
    IRealtimeNotificationSender realtimeSender,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<UserLevelChangedIntegrationEvent>
{
    private const string HandlerName = nameof(LevelUpEventHandler);

    public async Task HandleAsync(UserLevelChangedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var utcNow = dateTimeProvider.UtcNow;

        var notification = NotificationMessage.Create(
            @event.UserId,
            NotificationType.LevelUpCongratulations,
            "Level up approved",
            $"Your {@event.TargetLanguageCode.ToUpperInvariant()} level changed from {@event.PreviousLevel} to {@event.NewLevel}.",
            NotificationChannel.InApp,
            utcNow,
            JsonSerializer.Serialize(new
            {
                @event.TargetLanguageCode,
                @event.PreviousLevel,
                @event.NewLevel,
                @event.ChangedAtUtc
            }),
            utcNow);

        await notificationRepository.AddAsync(notification, ct);
        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await realtimeSender.SendNotificationAsync(@event.UserId, new NotificationPushPayload(
            notification.Id, notification.Type.ToString(), notification.Title, notification.Body, notification.ScheduledAtUtc), ct);
    }
}
