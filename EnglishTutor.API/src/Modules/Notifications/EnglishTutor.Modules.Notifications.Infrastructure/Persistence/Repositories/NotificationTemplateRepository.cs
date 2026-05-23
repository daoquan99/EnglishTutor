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

public sealed class NotificationTemplateRepository(NotificationsDbContext dbContext) : INotificationTemplateRepository
{
    public Task<NotificationTemplate?> GetActiveAsync(
        NotificationType type,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = languageCode.Trim().ToLowerInvariant();

        return dbContext.NotificationTemplates.SingleOrDefaultAsync(
            template => template.Type == type &&
                template.LanguageCode == normalizedLanguage &&
                template.IsActive,
            cancellationToken);
    }
}
