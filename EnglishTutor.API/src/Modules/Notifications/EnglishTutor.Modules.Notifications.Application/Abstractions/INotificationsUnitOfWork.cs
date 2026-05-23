namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface INotificationsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
