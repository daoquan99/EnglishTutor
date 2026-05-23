using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio) : ICommand<UserProfileResponse>;
