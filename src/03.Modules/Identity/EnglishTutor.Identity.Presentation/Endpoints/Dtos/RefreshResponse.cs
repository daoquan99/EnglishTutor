namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Refresh response: new JWT access token + access-token expiry.
/// </summary>
/// <remarks>
/// The rotated refresh token is NEVER returned in this body. It is delivered
/// only through the rotated HttpOnly <c>__Host-et_refresh</c> cookie
/// (Batch R1, H-01 / <c>security-identity.md</c>).
/// </remarks>
public sealed record RefreshResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);
