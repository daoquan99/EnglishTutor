namespace EnglishTutor.Modules.Users.Application.Abstractions;

public interface IUsersInboxStore
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken);
}
