namespace EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

public sealed record NotificationSettingsResponse(
    string TimeZone,
    QuietHoursDto QuietHours,
    ScheduleGroupDto StudyReminder,
    ScheduleGroupDto MissedStudyReminder,
    ScheduleGroupDto MistakeReviewReminder,
    ScheduleGroupDto VocabularyReviewReminder,
    ScheduleGroupDto WeeklySummary,
    ScheduleGroupDto MonthlySummary,
    ScheduleGroupDto AssessmentReminder);

public sealed record QuietHoursDto(bool Enabled, string? Start, string? End);

public sealed record ChannelsDto(bool InApp, bool Email, bool Push);

public sealed record ScheduleGroupDto(
    bool Enabled,
    ChannelsDto Channels,
    string? Time = null,
    int? BeforeMinutes = null,
    int? AfterMinutes = null,
    string? Frequency = null,
    string? DayOfWeek = null,
    int? DayOfMonth = null);
