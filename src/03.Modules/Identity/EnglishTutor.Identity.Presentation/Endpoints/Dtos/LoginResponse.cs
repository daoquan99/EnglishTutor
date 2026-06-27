namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Login response: JWT access token + access-token expiry.
/// </summary>
/// <remarks>
/// The refresh token is NEVER returned in this body. Browser auth transports
/// the refresh token only via the HttpOnly <c>__Host-et_refresh</c> cookie
/// (see <see cref="Auth.RefreshTokenCookieHelper"/>). Returning the raw secret
/// in JSON while a cookie is also set would defeat HttpOnly transport
/// (Batch R1, H-01 / <c>security-identity.md</c>).
/// </remarks>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);
