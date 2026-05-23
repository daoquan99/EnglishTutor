using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;
using EnglishTutor.Modules.Notifications.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Notifications.Application.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>
{
    public async Task<Result<IReadOnlyList<NotificationResponse>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var notifications = await notificationRepository.ListForUserAsync(
            request.UserId,
            page,
            pageSize,
            request.IsRead,
            cancellationToken);

        return notifications.Select(notification => notification.ToResponse()).ToArray();
    }
}
