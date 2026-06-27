using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Commands.Refresh;

/// <summary>
/// Refresh command. Carries the raw refresh-token value plus optional client
/// IP. Returns <see cref="RefreshResult"/> on success.
/// </summary>
public sealed record RefreshCommand(
    string RefreshToken,
    string? IpAddress) : ICommand<RefreshResult>;
