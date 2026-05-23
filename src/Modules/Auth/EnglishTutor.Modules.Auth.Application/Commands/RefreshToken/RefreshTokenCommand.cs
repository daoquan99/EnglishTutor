using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    Guid SessionId,
    string DeviceId,
    string? UserAgent,
    string? IpAddress) : ICommand<AuthTokenResponse>;
