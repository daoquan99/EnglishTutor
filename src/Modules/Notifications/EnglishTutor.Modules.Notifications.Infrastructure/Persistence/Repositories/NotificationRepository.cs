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

public sealed class NotificationRepository(NotificationsDbContext dbContext) : INotificationRepository
{
    public Task<NotificationMessage?> GetByIdForUserAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken) =>
        dbContext.NotificationMessages.SingleOrDefaultAsync(
            message => message.Id == notificationId && message.UserId == userId,
            cancellationToken);

    public async Task<IReadOnlyList<NotificationMessage>> ListForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isRead,
        CancellationToken cancellationToken)
    {
        var query = dbContext.NotificationMessages.Where(message => message.UserId == userId);
        if (isRead.HasValue)
        {
            query = query.Where(message => message.IsRead == isRead.Value);
        }

        return await query
            .OrderByDescending(message => message.ScheduledAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForUserOnDateAsync(
        Guid userId,
        NotificationType type,
        DateOnly scheduledDateUtc,
        CancellationToken cancellationToken)
    {
        var from = scheduledDateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = scheduledDateUtc.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return dbContext.NotificationMessages.AnyAsync(
            message => message.UserId == userId &&
                message.Type == type &&
                message.ScheduledAtUtc >= from &&
                message.ScheduledAtUtc < to,
            cancellationToken);
    }

    public async Task AddAsync(NotificationMessage message, CancellationToken cancellationToken) =>
        await dbContext.NotificationMessages.AddAsync(message, cancellationToken);
}
