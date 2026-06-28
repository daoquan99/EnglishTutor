using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string DisplayName,
    int Priority,
    IReadOnlyList<string> PermissionCodes) : ICommand<RoleResult>;
