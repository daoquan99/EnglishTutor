namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface IRealtimeNotificationSender
{
    Task SendNotificationAsync(Guid userId, NotificationPushPayload payload, CancellationToken cancellationToken = default);
}

public sealed record NotificationPushPayload(
    Guid Id,
    string Type,
    string Title,
    string Body,
    DateTime ScheduledAtUtc);
