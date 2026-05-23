namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface INotificationsInboxStore
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken);
}
