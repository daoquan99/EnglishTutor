using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;

namespace EnglishTutor.Identity.Application.Users.Commands.SetRolePermissions;

public sealed record SetRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyList<string> PermissionCodes) : ICommand<RoleResult>;
