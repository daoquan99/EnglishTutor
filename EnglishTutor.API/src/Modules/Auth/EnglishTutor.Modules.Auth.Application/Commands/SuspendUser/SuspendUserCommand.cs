using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.SuspendUser;

public sealed record SuspendUserCommand(Guid UserId, string? Reason) : ICommand;
