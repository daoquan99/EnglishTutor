using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Notifications.Application.Queries.GetNotificationSettings;

public sealed record GetNotificationSettingsQuery(Guid UserId) : IQuery<NotificationSettingsResponse>;
