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

public sealed class DailyStudyTargetCompletedEventHandler(
    INotificationRepository notificationRepository,
    INotificationTemplateRepository templateRepository,
    INotificationsInboxStore inboxStore,
    INotificationsUnitOfWork unitOfWork,
    IUserLanguageSettingsReader languageSettingsReader,
    IDateTimeProvider dateTimeProvider)
    : IIntegrationEventHandler<DailyStudyTargetCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(DailyStudyTargetCompletedEventHandler);

    public async Task HandleAsync(DailyStudyTargetCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var utcNow = dateTimeProvider.UtcNow;
        var languageCode = await GetUiLanguageCodeAsync(@event.UserId, ct);
        var template = await templateRepository.GetActiveAsync(NotificationType.DailyTargetCompleted, languageCode, ct)
            ?? await templateRepository.GetActiveAsync(NotificationType.DailyTargetCompleted, "en", ct);
        var values = new Dictionary<string, string>
        {
            ["actualMinutes"] = @event.ActualMinutes.ToString(),
            ["targetMinutes"] = @event.TargetMinutes.ToString(),
            ["targetLanguageCode"] = @event.TargetLanguageCode,
            ["date"] = @event.CompletedDateUtc.ToString("yyyy-MM-dd")
        };

        var title = template is null
            ? "Daily target completed"
            : NotificationTemplateRenderer.Render(template.TitleTemplate, values);
        var body = template is null
            ? $"You studied {@event.ActualMinutes} minutes today."
            : NotificationTemplateRenderer.Render(template.BodyTemplate, values);

        await notificationRepository.AddAsync(NotificationMessage.Create(
            @event.UserId,
            NotificationType.DailyTargetCompleted,
            title,
            body,
            NotificationChannel.InApp,
            utcNow,
            JsonSerializer.Serialize(new
            {
                @event.TargetLanguageCode,
                @event.ActualMinutes,
                @event.TargetMinutes,
                @event.CompletedDateUtc
            }),
            utcNow), ct);

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
