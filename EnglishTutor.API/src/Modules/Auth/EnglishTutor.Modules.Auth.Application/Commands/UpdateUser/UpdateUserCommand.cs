using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string DisplayName) : ICommand;
