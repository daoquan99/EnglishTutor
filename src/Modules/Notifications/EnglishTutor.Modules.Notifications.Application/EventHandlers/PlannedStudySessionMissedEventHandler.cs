using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.Services;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Notifications.Application.EventHandlers;

public sealed class PlannedStudySessionMissedEventHandler(
    INotificationRepository notificationRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationTemplateRepository templateRepository,
    INotificationsInboxStore inboxStore,
    INotificationsUnitOfWork unitOfWork,
    IUserLanguageSettingsReader languageSettingsReader,
    IRealtimeNotificationSender realtimeSender,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<PlannedStudySessionMissedIntegrationEvent>
{
    private const string HandlerName = nameof(PlannedStudySessionMissedEventHandler);

    public async Task HandleAsync(PlannedStudySessionMissedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var utcNow = dateTimeProvider.UtcNow;
        var schedule = await scheduleRepository.GetByUserAndTypeAsync(
            @event.UserId, NotificationType.MissedStudyReminder, cancellationToken: ct);

        if (schedule is not null && schedule.IsEnabled)
        {
            var languageCode = await GetUiLanguageCodeAsync(@event.UserId, ct);
            var template = await templateRepository.GetActiveAsync(NotificationType.MissedStudyReminder, languageCode, ct)
                ?? await templateRepository.GetActiveAsync(NotificationType.MissedStudyReminder, "en", ct);
            var values = new Dictionary<string, string>
            {
                ["date"] = @event.ScheduledDateUtc.ToString("yyyy-MM-dd"),
                ["studyTime"] = @event.ScheduledDateUtc.ToString("HH:mm"),
                ["targetLanguageCode"] = @event.TargetLanguageCode
            };

            var title = template is null
                ? "You missed today's session"
                : NotificationTemplateRenderer.Render(template.TitleTemplate, values);
            var body = template is null
                ? "Open your study plan and recover the missed practice session."
                : NotificationTemplateRenderer.Render(template.BodyTemplate, values);

            var notification = NotificationMessage.Create(
                @event.UserId,
                NotificationType.MissedStudyReminder,
                title,
                body,
                schedule.GetPreferredChannel(),
                utcNow,
                JsonSerializer.Serialize(new
                {
                    @event.SessionId,
                    @event.TargetLanguageCode,
                    @event.ScheduledDateUtc
                }),
                utcNow);

            await notificationRepository.AddAsync(notification, ct);
            await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await realtimeSender.SendNotificationAsync(@event.UserId, new NotificationPushPayload(
                notification.Id, notification.Type.ToString(), notification.Title, notification.Body, notification.ScheduledAtUtc), ct);

            return;
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<string> GetUiLanguageCodeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(userId, cancellationToken);
        return string.IsNullOrWhiteSpace(settings?.UiLanguageCode)
            ? "en"
            : settings.UiLanguageCode;
    }
}
