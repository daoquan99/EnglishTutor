using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Users.Application.Commands.CreateUserProfile;

public sealed record CreateUserProfileCommand(Guid UserId, string DisplayName) : ICommand;
