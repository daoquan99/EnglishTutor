using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.Shared.Mappers;

internal static class NotificationMappers
{
    public static NotificationResponse ToResponse(this NotificationMessage message) =>
        new(
            message.Id,
            message.Type.ToString(),
            message.Title,
            message.Body,
            message.Data,
            message.IsRead,
            message.Channel.ToString(),
            message.Status.ToString(),
            message.ScheduledAtUtc,
            message.SentAtUtc,
            message.ReadAtUtc);

    public static NotificationSettingsResponse ToSettingsResponse(
        NotificationSetting settings,
        List<UserNotificationSchedule> schedules)
    {
        var lookup = schedules.ToDictionary(s => s.NotificationType);

        return new NotificationSettingsResponse(
            settings.TimeZone,
            new QuietHoursDto(
                settings.QuietHoursEnabled,
                settings.QuietHoursStart?.ToString("HH:mm"),
                settings.QuietHoursEnd?.ToString("HH:mm")),
            MapSchedule(lookup, NotificationType.StudyReminder),
            MapSchedule(lookup, NotificationType.MissedStudyReminder),
            MapSchedule(lookup, NotificationType.MistakeReviewReminder),
            MapSchedule(lookup, NotificationType.VocabularyReviewReminder),
            MapSchedule(lookup, NotificationType.WeeklyProgressSummary),
            MapSchedule(lookup, NotificationType.MonthlyProgressSummary),
            MapSchedule(lookup, NotificationType.AssessmentReminder));
    }

    private static ScheduleGroupDto MapSchedule(
        Dictionary<NotificationType, UserNotificationSchedule> lookup,
        NotificationType type)
    {
        if (!lookup.TryGetValue(type, out var schedule))
        {
            return new ScheduleGroupDto(
                false,
                new ChannelsDto(true, false, false));
        }

        return new ScheduleGroupDto(
            schedule.IsEnabled,
            new ChannelsDto(schedule.InAppEnabled, schedule.EmailEnabled, schedule.PushEnabled),
            schedule.PreferredTime?.ToString("HH:mm"),
            schedule.ReminderBeforeMinutes,
            schedule.RemindAfterMinutes,
            schedule.Frequency?.ToString(),
            schedule.DayOfWeek?.ToString(),
            schedule.DayOfMonth);
    }
}
