using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;

namespace EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;

public sealed class UserNotificationSchedule : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string? TargetLanguageCode { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public bool IsEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public TimeOnly? PreferredTime { get; private set; }
    public int? ReminderBeforeMinutes { get; private set; }
    public int? RemindAfterMinutes { get; private set; }
    public NotificationFrequency? Frequency { get; private set; }
    public DayOfWeek? DayOfWeek { get; private set; }
    public int? DayOfMonth { get; private set; }

    private UserNotificationSchedule() { }

    public static UserNotificationSchedule CreateDefault(Guid userId, NotificationType type, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserNotificationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationType = type,
            IsEnabled = false,
            InAppEnabled = true,
            EmailEnabled = false,
            PushEnabled = false,
            CreatedAtUtc = utcNow
        };
    }

    public void Enable()
    {
        if (!HasAnyChannelEnabled())
        {
            throw new DomainException("At least one delivery channel must be enabled.");
        }

        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void SetChannels(bool inApp, bool email, bool push)
    {
        if (IsEnabled && !inApp && !email && !push)
        {
            throw new DomainException("At least one delivery channel must be enabled when the notification is active.");
        }

        InAppEnabled = inApp;
        EmailEnabled = email;
        PushEnabled = push;
    }

    public void SetTiming(
        TimeOnly? preferredTime,
        int? reminderBeforeMinutes,
        int? remindAfterMinutes,
        NotificationFrequency? frequency,
        DayOfWeek? dayOfWeek,
        int? dayOfMonth)
    {
        PreferredTime = preferredTime;
        ReminderBeforeMinutes = reminderBeforeMinutes;
        RemindAfterMinutes = remindAfterMinutes;
        Frequency = frequency;
        DayOfWeek = dayOfWeek;
        DayOfMonth = dayOfMonth;
    }

    public bool HasAnyChannelEnabled() => InAppEnabled || EmailEnabled || PushEnabled;

    public Shared.NotificationChannel GetPreferredChannel()
    {
        if (InAppEnabled) return Shared.NotificationChannel.InApp;
        if (EmailEnabled) return Shared.NotificationChannel.Email;
        return Shared.NotificationChannel.Push;
    }
}
