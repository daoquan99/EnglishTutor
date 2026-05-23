using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Notifications.Application.Abstractions;

namespace EnglishTutor.Modules.Notifications.Application.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(
    INotificationRepository notificationRepository,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<Result<int>> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await notificationRepository.GetUnreadForUserAsync(request.UserId, cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = dateTimeProvider.UtcNow;
        foreach (var notification in unread)
        {
            notification.MarkAsRead(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return unread.Count;
    }
}
