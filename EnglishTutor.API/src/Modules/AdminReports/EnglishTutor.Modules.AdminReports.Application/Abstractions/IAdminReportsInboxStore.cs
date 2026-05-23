namespace EnglishTutor.Modules.AdminReports.Application.Abstractions;

public interface IAdminReportsInboxStore
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken);
}
