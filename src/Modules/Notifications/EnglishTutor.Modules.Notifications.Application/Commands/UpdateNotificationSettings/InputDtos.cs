namespace EnglishTutor.Modules.Notifications.Application.Commands.UpdateNotificationSettings;

public sealed record QuietHoursInput(bool Enabled, string? Start, string? End);

public sealed record ChannelsInput(bool InApp, bool Email, bool Push);

public sealed record ScheduleInput(
    bool Enabled,
    ChannelsInput Channels,
    string? Time = null,
    int? BeforeMinutes = null,
    int? AfterMinutes = null,
    string? Frequency = null,
    string? DayOfWeek = null,
    int? DayOfMonth = null);
