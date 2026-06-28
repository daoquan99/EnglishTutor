using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(
    Guid UserId,
    bool IsActive,
    bool ActorIsOwner) : ICommand;
