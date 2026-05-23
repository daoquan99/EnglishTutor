using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Notifications.Application.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : ICommand<NotificationResponse>;
