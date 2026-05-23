using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;

public sealed class NotificationDeliveryLog : Entity<Guid>
{
    public Guid NotificationMessageId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }

    private NotificationDeliveryLog() { }

    public static NotificationDeliveryLog Create(
        Guid notificationMessageId,
        NotificationChannel channel,
        NotificationStatus status,
        string? errorMessage,
        DateTime attemptedAtUtc)
    {
        if (notificationMessageId == Guid.Empty)
        {
            throw new DomainException("Notification message id is required.");
        }

        if (status is not NotificationStatus.Sent and not NotificationStatus.Failed)
        {
            throw new DomainException("Delivery log status must be Sent or Failed.");
        }

        return new NotificationDeliveryLog
        {
            Id = Guid.NewGuid(),
            NotificationMessageId = notificationMessageId,
            Channel = channel,
            Status = status,
            ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            AttemptedAtUtc = attemptedAtUtc,
            CreatedAtUtc = attemptedAtUtc
        };
    }
}
