using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Commands.Logout;

/// <summary>
/// Logout command. Revokes the entire refresh-token family so subsequent
/// refresh attempts fail. Idempotent: if the token is already revoked, the
/// command succeeds silently. Uses the non-generic <see cref="ICommand"/>
/// contract — the handler returns <see cref="Result"/> (no response value).
/// </summary>
public sealed record LogoutCommand(
    string RefreshToken) : ICommand;
