using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;

namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface INotificationSettingRepository
{
    Task<NotificationSetting?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(NotificationSetting setting, CancellationToken cancellationToken);
}
