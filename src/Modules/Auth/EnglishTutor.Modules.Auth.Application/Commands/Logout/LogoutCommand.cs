using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.Logout;

public sealed record LogoutCommand(
    Guid UserId,
    string RefreshToken,
    Guid SessionId,
    string DeviceId) : ICommand;
