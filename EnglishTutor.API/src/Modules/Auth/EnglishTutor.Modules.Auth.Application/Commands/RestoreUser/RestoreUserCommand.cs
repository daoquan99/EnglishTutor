using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.RestoreUser;

public sealed record RestoreUserCommand(Guid UserId) : ICommand;
