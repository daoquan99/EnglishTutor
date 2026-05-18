using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string DeviceId,
    string? UserAgent,
    string? IpAddress) : ICommand<AuthTokenResponse>;
