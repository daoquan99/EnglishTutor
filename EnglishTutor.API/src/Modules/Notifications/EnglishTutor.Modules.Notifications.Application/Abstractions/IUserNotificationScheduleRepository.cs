using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.Abstractions;

public interface IUserNotificationScheduleRepository
{
    Task<UserNotificationSchedule?> GetByUserAndTypeAsync(
        Guid userId, NotificationType type, string? languageCode = null, CancellationToken cancellationToken = default);

    Task<List<UserNotificationSchedule>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserNotificationSchedule schedule, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<UserNotificationSchedule> schedules, CancellationToken cancellationToken = default);
}
