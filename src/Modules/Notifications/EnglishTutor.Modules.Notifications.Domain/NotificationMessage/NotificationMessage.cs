using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Domain.NotificationMessage;

public sealed class NotificationMessage : AggregateRoot<Guid>
{
    private readonly List<NotificationDeliveryLog> _deliveryLogs = [];

    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Data { get; private set; }
    public bool IsRead { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public IReadOnlyCollection<NotificationDeliveryLog> DeliveryLogs => _deliveryLogs.AsReadOnly();

    private NotificationMessage() { }

    public static NotificationMessage Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        NotificationChannel channel,
        DateTime scheduledAtUtc,
        string? data,
        DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new NotificationMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = NormalizeRequired(title, 200, "Notification title"),
            Body = NormalizeRequired(body, 2000, "Notification body"),
            Data = string.IsNullOrWhiteSpace(data) ? null : data.Trim(),
            Channel = channel,
            Status = NotificationStatus.Pending,
            ScheduledAtUtc = scheduledAtUtc,
            CreatedAtUtc = utcNow
        };
    }

    public void MarkAsRead(DateTime utcNow)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = utcNow;
        Status = NotificationStatus.Read;
        UpdatedAtUtc = utcNow;
    }

    public void MarkAsSent(DateTime utcNow)
    {
        if (Status == NotificationStatus.Read)
        {
            return;
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = utcNow;
        LastErrorMessage = null;
        UpdatedAtUtc = utcNow;
        _deliveryLogs.Add(NotificationDeliveryLog.Create(Id, Channel, NotificationStatus.Sent, null, utcNow));
    }

    public void MarkAsFailed(string errorMessage, DateTime utcNow)
    {
        Status = NotificationStatus.Failed;
        LastErrorMessage = NormalizeRequired(errorMessage, 1000, "Delivery error message");
        UpdatedAtUtc = utcNow;
        _deliveryLogs.Add(NotificationDeliveryLog.Create(Id, Channel, NotificationStatus.Failed, LastErrorMessage, utcNow));
    }

    private static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
