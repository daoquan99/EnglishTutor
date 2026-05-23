using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationScheduleRepository(NotificationsDbContext dbContext)
    : IUserNotificationScheduleRepository
{
    public async Task<UserNotificationSchedule?> GetByUserAndTypeAsync(
        Guid userId, NotificationType type, string? languageCode = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserNotificationSchedules
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.NotificationType == type &&
                s.TargetLanguageCode == languageCode,
                cancellationToken);
    }

    public async Task<List<UserNotificationSchedule>> GetAllByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserNotificationSchedules
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserNotificationSchedule schedule, CancellationToken cancellationToken = default)
    {
        await dbContext.UserNotificationSchedules.AddAsync(schedule, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<UserNotificationSchedule> schedules, CancellationToken cancellationToken = default)
    {
        await dbContext.UserNotificationSchedules.AddRangeAsync(schedules, cancellationToken);
    }
}
