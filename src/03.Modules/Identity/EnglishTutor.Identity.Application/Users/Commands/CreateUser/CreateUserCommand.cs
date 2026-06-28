using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    Guid? CreatedByUserId,
    bool ActorIsOwner) : ICommand<UserDetailResult>;
