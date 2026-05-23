using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Notifications.Application.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    int Page,
    int PageSize,
    bool? IsRead) : IQuery<IReadOnlyList<NotificationResponse>>;
