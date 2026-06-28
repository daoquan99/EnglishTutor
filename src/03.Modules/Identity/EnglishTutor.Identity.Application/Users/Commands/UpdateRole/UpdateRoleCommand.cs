using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string DisplayName,
    int Priority) : ICommand<RoleResult>;
