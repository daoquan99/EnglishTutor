using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Notifications.Application.Commands.UpdateNotificationSettings;

public sealed record UpdateNotificationSettingsCommand(
    Guid UserId,
    string TimeZone,
    QuietHoursInput QuietHours,
    ScheduleInput StudyReminder,
    ScheduleInput MissedStudyReminder,
    ScheduleInput MistakeReviewReminder,
    ScheduleInput VocabularyReviewReminder,
    ScheduleInput WeeklySummary,
    ScheduleInput MonthlySummary,
    ScheduleInput AssessmentReminder) : ICommand<NotificationSettingsResponse>;
