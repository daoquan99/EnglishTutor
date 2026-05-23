using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.Services;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using Quartz;

namespace EnglishTutor.Worker.Jobs;

[DisallowConcurrentExecution]
public sealed class StudyReminderSchedulingJob(
    IStudyPlanRepository studyPlanRepository,
    INotificationRepository notificationRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationTemplateRepository templateRepository,
    INotificationsUnitOfWork notificationsUnitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<StudyReminderSchedulingJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var windowEndUtc = utcNow.AddMinutes(5);
        var plans = await studyPlanRepository.ListActiveAsync(context.CancellationToken);
        var createdCount = 0;

        foreach (var plan in plans)
        {
            foreach (var reminder in GetReminderCandidates(plan, utcNow, windowEndUtc, logger))
            {
                if (await notificationRepository.ExistsForUserOnDateAsync(
                        plan.UserId,
                        NotificationType.StudyReminder,
                        reminder.LocalStudyDate,
                        context.CancellationToken))
                {
                    continue;
                }

                var schedule = await scheduleRepository.GetByUserAndTypeAsync(
                    plan.UserId, NotificationType.StudyReminder, cancellationToken: context.CancellationToken);
                if (schedule is null || !schedule.IsEnabled)
                {
                    continue;
                }

                var template = await templateRepository.GetActiveAsync(NotificationType.StudyReminder, "en", context.CancellationToken);
                var values = new Dictionary<string, string>
                {
                    ["targetLanguageCode"] = plan.TargetLanguageCode.Value,
                    ["studyTime"] = reminder.LocalStudyTime.ToString("HH:mm")
                };

                var title = template is null
                    ? "Time to study!"
                    : NotificationTemplateRenderer.Render(template.TitleTemplate, values);
                var body = template is null
                    ? $"Your {plan.TargetLanguageCode.Value} session starts at {reminder.LocalStudyTime:HH:mm}."
                    : NotificationTemplateRenderer.Render(template.BodyTemplate, values);

                await notificationRepository.AddAsync(NotificationMessage.Create(
                    plan.UserId,
                    NotificationType.StudyReminder,
                    title,
                    body,
                    schedule.GetPreferredChannel(),
                    reminder.ReminderAtUtc,
                    null,
                    utcNow), context.CancellationToken);
                createdCount++;
            }
        }

        if (createdCount > 0)
        {
            await notificationsUnitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        logger.LogInformation("Created {Count} study reminder notifications at {UtcNow}", createdCount, utcNow);
    }

    private static IEnumerable<ReminderCandidate> GetReminderCandidates(
        UserStudyPlan plan,
        DateTime utcNow,
        DateTime windowEndUtc,
        ILogger logger)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(plan.TimeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            logger.LogWarning(exception, "Skipping study reminder for plan {StudyPlanId} because time zone {TimeZoneId} was not found.", plan.Id, plan.TimeZoneId);
            yield break;
        }
        catch (InvalidTimeZoneException exception)
        {
            logger.LogWarning(exception, "Skipping study reminder for plan {StudyPlanId} because time zone {TimeZoneId} is invalid.", plan.Id, plan.TimeZoneId);
            yield break;
        }

        var studyDays = plan.WeekDays
            .Where(day => day.IsStudyDay)
            .Select(day => day.DayOfWeek)
            .ToHashSet();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), timeZone);
        var localWindowEnd = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(windowEndUtc, DateTimeKind.Utc), timeZone);
        var localDates = new[] { DateOnly.FromDateTime(localNow), DateOnly.FromDateTime(localWindowEnd) }
            .Distinct()
            .ToArray();

        foreach (var localDate in localDates)
        {
            if (!studyDays.Contains(localDate.DayOfWeek))
            {
                continue;
            }

            var localStudyTime = plan.PreferredStudyTime;
            var localStudyDateTime = localDate.ToDateTime(localStudyTime, DateTimeKind.Unspecified);
            var localReminderDateTime = localStudyDateTime.AddMinutes(-plan.ReminderBeforeMinutes);

            if (timeZone.IsInvalidTime(localStudyDateTime) || timeZone.IsInvalidTime(localReminderDateTime))
            {
                logger.LogWarning(
                    "Skipping study reminder for plan {StudyPlanId} because local study time {LocalStudyDateTime} is invalid in time zone {TimeZoneId}.",
                    plan.Id,
                    localStudyDateTime,
                    plan.TimeZoneId);
                continue;
            }

            var reminderAtUtc = TimeZoneInfo.ConvertTimeToUtc(localReminderDateTime, timeZone);
            if (reminderAtUtc < utcNow || reminderAtUtc > windowEndUtc)
            {
                continue;
            }

            yield return new ReminderCandidate(localDate, localStudyTime, reminderAtUtc);
        }
    }

    private sealed record ReminderCandidate(DateOnly LocalStudyDate, TimeOnly LocalStudyTime, DateTime ReminderAtUtc);
}
