using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Notifications.Domain.NotificationSetting;

public sealed class NotificationSetting : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public bool QuietHoursEnabled { get; private set; }
    public TimeOnly? QuietHoursStart { get; private set; }
    public TimeOnly? QuietHoursEnd { get; private set; }

    private NotificationSetting() { }

    public static NotificationSetting Create(Guid userId, string timeZone, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (string.IsNullOrWhiteSpace(timeZone))
        {
            throw new DomainException("Time zone is required.");
        }

        if (timeZone.Length > 100)
        {
            throw new DomainException("Time zone must not exceed 100 characters.");
        }

        return new NotificationSetting
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TimeZone = timeZone.Trim(),
            QuietHoursEnabled = false,
            CreatedAtUtc = utcNow
        };
    }

    public void UpdateTimeZone(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            throw new DomainException("Time zone is required.");
        }

        if (timeZone.Length > 100)
        {
            throw new DomainException("Time zone must not exceed 100 characters.");
        }

        TimeZone = timeZone.Trim();
    }

    public void SetQuietHours(bool enabled, TimeOnly? start, TimeOnly? end)
    {
        if (enabled && (start is null || end is null))
        {
            throw new DomainException("Quiet hours start and end are required when quiet hours are enabled.");
        }

        QuietHoursEnabled = enabled;
        QuietHoursStart = enabled ? start : null;
        QuietHoursEnd = enabled ? end : null;
    }
}
