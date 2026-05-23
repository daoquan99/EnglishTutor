using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Persistence.Repositories;

public sealed class NotificationSettingRepository(NotificationsDbContext dbContext) : INotificationSettingRepository
{
    public Task<NotificationSetting?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.NotificationSettings.SingleOrDefaultAsync(setting => setting.UserId == userId, cancellationToken);

    public async Task AddAsync(NotificationSetting setting, CancellationToken cancellationToken) =>
        await dbContext.NotificationSettings.AddAsync(setting, cancellationToken);
}
