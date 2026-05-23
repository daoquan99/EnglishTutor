namespace EnglishTutor.Modules.Notifications.Presentation.Requests;

public sealed record UpdateNotificationSettingsRequest(
    string TimeZone,
    QuietHoursRequest QuietHours,
    ScheduleRequest StudyReminder,
    ScheduleRequest MissedStudyReminder,
    ScheduleRequest MistakeReviewReminder,
    ScheduleRequest VocabularyReviewReminder,
    ScheduleRequest WeeklySummary,
    ScheduleRequest MonthlySummary,
    ScheduleRequest AssessmentReminder);

public sealed record QuietHoursRequest(bool Enabled, string? Start, string? End);

public sealed record ChannelsRequest(bool InApp, bool Email, bool Push);

public sealed record ScheduleRequest(
    bool Enabled,
    ChannelsRequest Channels,
    string? Time = null,
    int? BeforeMinutes = null,
    int? AfterMinutes = null,
    string? Frequency = null,
    string? DayOfWeek = null,
    int? DayOfMonth = null);
