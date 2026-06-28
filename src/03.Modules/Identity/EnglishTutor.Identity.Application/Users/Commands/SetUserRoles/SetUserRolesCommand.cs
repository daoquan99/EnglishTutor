using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(
    Guid UserId,
    IReadOnlyList<string> Roles,
    bool ActorIsOwner) : ICommand<UserDetailResult>;
