using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string DisplayName,
    string DeviceId,
    string? UserAgent,
    string? IpAddress) : ICommand<AuthTokenResponse>;
