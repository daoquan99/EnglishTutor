using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Errors;

/// <summary>
/// Domain error factories for session-related failures. Used by aggregate
/// invariants and (in Slice 2.6) by handlers that translate them into
/// Application-layer <c>Result</c> failures.
/// </summary>
public static class SessionErrors
{
    public static Error NotFound(Guid sessionId) =>
        new("Identity.SessionNotFound", $"Session '{sessionId}' was not found.");

    public static Error AlreadyRevoked() =>
        new("Identity.SessionAlreadyRevoked", "Session is already revoked.");

    public static Error RefreshTokenNotFound() =>
        new("Identity.RefreshTokenNotFound", "Refresh token was not found.");

    public static Error RefreshTokenExpired() =>
        new("Identity.RefreshTokenExpired", "Refresh token is expired or revoked.");

    public static Error RefreshTokenReuseDetected(Guid familyId) =>
        new("Identity.RefreshTokenReuse",
            $"Refresh-token reuse detected (family {familyId}). All sessions revoked.");
}
