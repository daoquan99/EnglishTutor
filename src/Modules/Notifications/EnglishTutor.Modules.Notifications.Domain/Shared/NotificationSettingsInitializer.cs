using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;

namespace EnglishTutor.Modules.Notifications.Domain.Shared;

public static class NotificationSettingsInitializer
{
    private static readonly NotificationType[] ConfigurableTypes =
    [
        NotificationType.StudyReminder,
        NotificationType.MissedStudyReminder,
        NotificationType.MistakeReviewReminder,
        NotificationType.VocabularyReviewReminder,
        NotificationType.WeeklyProgressSummary,
        NotificationType.MonthlyProgressSummary,
        NotificationType.AssessmentReminder
    ];

    public static (NotificationSetting.NotificationSetting Settings, List<UserNotificationSchedule.UserNotificationSchedule> Schedules)
        CreateDefaults(Guid userId, string timeZone, DateTime utcNow)
    {
        var settings = NotificationSetting.NotificationSetting.Create(userId, timeZone, utcNow);

        var schedules = ConfigurableTypes
            .Select(type => UserNotificationSchedule.UserNotificationSchedule.CreateDefault(userId, type, utcNow))
            .ToList();

        return (settings, schedules);
    }

    public static List<UserNotificationSchedule.UserNotificationSchedule> CreateMissingSchedules(
        Guid userId,
        IReadOnlyCollection<UserNotificationSchedule.UserNotificationSchedule> existingSchedules,
        DateTime utcNow)
    {
        var existingTypes = existingSchedules
            .Select(s => s.NotificationType)
            .ToHashSet();

        return ConfigurableTypes
            .Where(type => !existingTypes.Contains(type))
            .Select(type => UserNotificationSchedule.UserNotificationSchedule.CreateDefault(userId, type, utcNow))
            .ToList();
    }
}
