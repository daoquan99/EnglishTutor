using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface INotificationRepository
{
    Task<NotificationMessage?> GetByIdForUserAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationMessage>> ListForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isRead,
        CancellationToken cancellationToken);

    Task<bool> ExistsForUserOnDateAsync(
        Guid userId,
        NotificationType type,
        DateOnly scheduledDateUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationMessage>> GetUnreadForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(NotificationMessage message, CancellationToken cancellationToken);
}
