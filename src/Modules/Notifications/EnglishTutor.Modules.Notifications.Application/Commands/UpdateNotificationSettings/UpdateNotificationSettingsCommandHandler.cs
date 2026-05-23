using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;
using EnglishTutor.Modules.Notifications.Application.Shared.Mappers;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.Commands.UpdateNotificationSettings;

public sealed class UpdateNotificationSettingsCommandHandler(
    INotificationSettingRepository settingRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateNotificationSettingsCommand, NotificationSettingsResponse>
{
    public async Task<Result<NotificationSettingsResponse>> Handle(
        UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;

        var setting = await settingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        List<UserNotificationSchedule> schedules;

        if (setting is null)
        {
            var defaults = NotificationSettingsInitializer.CreateDefaults(request.UserId, request.TimeZone, utcNow);
            setting = defaults.Settings;
            schedules = defaults.Schedules;
            await settingRepository.AddAsync(setting, cancellationToken);
            await scheduleRepository.AddRangeAsync(schedules, cancellationToken);
        }
        else
        {
            schedules = await scheduleRepository.GetAllByUserAsync(request.UserId, cancellationToken);

            var missing = NotificationSettingsInitializer.CreateMissingSchedules(request.UserId, schedules, utcNow);
            if (missing.Count > 0)
            {
                await scheduleRepository.AddRangeAsync(missing, cancellationToken);
                schedules.AddRange(missing);
            }
        }

        setting.UpdateTimeZone(request.TimeZone);
        setting.SetQuietHours(
            request.QuietHours.Enabled,
            ParseTime(request.QuietHours.Start),
            ParseTime(request.QuietHours.End));

        ApplyScheduleInput(schedules, NotificationType.StudyReminder, request.StudyReminder);
        ApplyScheduleInput(schedules, NotificationType.MissedStudyReminder, request.MissedStudyReminder);
        ApplyScheduleInput(schedules, NotificationType.MistakeReviewReminder, request.MistakeReviewReminder);
        ApplyScheduleInput(schedules, NotificationType.VocabularyReviewReminder, request.VocabularyReviewReminder);
        ApplyScheduleInput(schedules, NotificationType.WeeklyProgressSummary, request.WeeklySummary);
        ApplyScheduleInput(schedules, NotificationType.MonthlyProgressSummary, request.MonthlySummary);
        ApplyScheduleInput(schedules, NotificationType.AssessmentReminder, request.AssessmentReminder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NotificationMappers.ToSettingsResponse(setting, schedules);
    }

    private static void ApplyScheduleInput(
        List<UserNotificationSchedule> schedules,
        NotificationType type,
        ScheduleInput input)
    {
        var schedule = schedules.FirstOrDefault(s => s.NotificationType == type);
        if (schedule is null) return;

        schedule.SetChannels(input.Channels.InApp, input.Channels.Email, input.Channels.Push);

        schedule.SetTiming(
            ParseTime(input.Time),
            input.BeforeMinutes,
            input.AfterMinutes,
            ParseFrequency(input.Frequency),
            ParseDayOfWeek(input.DayOfWeek),
            input.DayOfMonth);

        if (input.Enabled)
        {
            schedule.Enable();
        }
        else
        {
            schedule.Disable();
        }
    }

    private static TimeOnly? ParseTime(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : TimeOnly.Parse(value);

    private static NotificationFrequency? ParseFrequency(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<NotificationFrequency>(value, ignoreCase: true);

    private static DayOfWeek? ParseDayOfWeek(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<DayOfWeek>(value, ignoreCase: true);
}
