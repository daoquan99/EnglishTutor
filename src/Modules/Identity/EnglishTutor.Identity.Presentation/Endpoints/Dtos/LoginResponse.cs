namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Login response: JWT access token + refresh token + access-token expiry.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
