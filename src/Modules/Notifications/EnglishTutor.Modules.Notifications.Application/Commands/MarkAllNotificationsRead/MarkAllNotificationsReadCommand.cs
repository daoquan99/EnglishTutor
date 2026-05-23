using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Notifications.Application.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(Guid UserId) : ICommand<int>;
