using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string DisplayName,
    bool IsActive,
    bool ActorIsOwner) : ICommand<UserDetailResult>;
