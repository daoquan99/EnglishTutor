namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Refresh response: new JWT access token + rotated refresh token + expiry.
/// </summary>
public sealed record RefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
