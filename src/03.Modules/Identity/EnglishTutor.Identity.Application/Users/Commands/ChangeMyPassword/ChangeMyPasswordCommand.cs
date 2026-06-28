using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Users.Commands.ChangeMyPassword;

public sealed record ChangeMyPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand;
