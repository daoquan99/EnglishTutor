using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using MediatR;

namespace EnglishTutor.Identity.Application.Commands.Logout;

/// <summary>
/// Logout command. Revokes the entire refresh-token family so subsequent
/// refresh attempts fail. Idempotent: if the token is already revoked, the
/// command succeeds silently.
/// </summary>
public sealed record LogoutCommand(
    string RefreshToken) : ICommand<Unit>;

public sealed class LogoutCommandMarker { }
