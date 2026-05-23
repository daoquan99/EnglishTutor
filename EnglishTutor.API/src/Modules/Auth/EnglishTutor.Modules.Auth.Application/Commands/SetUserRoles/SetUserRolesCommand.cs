using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyCollection<Guid> RoleIds) : ICommand;
