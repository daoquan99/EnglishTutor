using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.AdminReports.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.EventHandlers;

public sealed class ProgressSummaryReadyEventHandler(
    INotificationRepository notificationRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationsInboxStore inboxStore,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<ProgressSummaryReadyIntegrationEvent>
{
    private const string HandlerName = nameof(ProgressSummaryReadyEventHandler);

    public async Task HandleAsync(ProgressSummaryReadyIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var notificationType = @event.Period switch
        {
            "Weekly" => NotificationType.WeeklyProgressSummary,
            "Monthly" => NotificationType.MonthlyProgressSummary,
            _ => throw new InvalidOperationException($"Progress summary period '{@event.Period}' is unsupported.")
        };

        var utcNow = dateTimeProvider.UtcNow;
        var schedule = await scheduleRepository.GetByUserAndTypeAsync(@event.UserId, notificationType, cancellationToken: ct);

        var scheduledDate = DateOnly.FromDateTime(@event.GeneratedAtUtc);
        if (schedule is not null && schedule.IsEnabled &&
            !await notificationRepository.ExistsForUserOnDateAsync(@event.UserId, notificationType, scheduledDate, ct))
        {
            await notificationRepository.AddAsync(NotificationMessage.Create(
                @event.UserId,
                notificationType,
                notificationType == NotificationType.WeeklyProgressSummary
                    ? "Your weekly progress summary is ready"
                    : "Your monthly progress summary is ready",
                $"You completed {@event.ActivityCount} learning activities from {@event.StartDate:yyyy-MM-dd} to {@event.EndDate:yyyy-MM-dd}.",
                schedule.GetPreferredChannel(),
                @event.GeneratedAtUtc,
                JsonSerializer.Serialize(new
                {
                    startDate = @event.StartDate,
                    endDate = @event.EndDate,
                    activityCount = @event.ActivityCount,
                    expEarned = @event.ExpEarned,
                    generatedAtUtc = @event.GeneratedAtUtc
                }),
                utcNow), ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
