using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;
using EnglishTutor.Modules.Notifications.Application.Shared.Errors;
using EnglishTutor.Modules.Notifications.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Notifications.Application.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(
    INotificationRepository notificationRepository,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<MarkNotificationReadCommand, NotificationResponse>
{
    public async Task<Result<NotificationResponse>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdForUserAsync(
            request.NotificationId,
            request.UserId,
            cancellationToken);

        if (notification is null)
        {
            return Result.Failure<NotificationResponse>(NotificationErrors.NotificationNotFound(request.NotificationId));
        }

        notification.MarkAsRead(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return notification.ToResponse();
    }
}
