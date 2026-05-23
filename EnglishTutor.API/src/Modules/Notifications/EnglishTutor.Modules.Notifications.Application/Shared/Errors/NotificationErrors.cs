using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Notifications.Application.Shared.Errors;

public static class NotificationErrors
{
    public static Error NotificationNotFound(Guid notificationId) =>
        Error.NotFound("Notification", notificationId);

    public static readonly Error SettingsNotFound =
        Error.NotFound("Notification settings were not found for the current user.");

    public static readonly Error InvalidTimeZone =
        Error.Validation("Invalid time zone identifier.");

    public static readonly Error QuietHoursIncomplete =
        Error.Validation("Quiet hours start and end are required when quiet hours are enabled.");

    public static readonly Error NoChannelEnabled =
        Error.Validation("At least one delivery channel must be enabled for an active notification type.");
}
