using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeleteRole;

public sealed record DeleteAuthRoleCommand(Guid RoleId) : ICommand;
