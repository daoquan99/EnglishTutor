using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(
    Guid UserId,
    string DisplayName) : ICommand<CurrentUserResult>;
