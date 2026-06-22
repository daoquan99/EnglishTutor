namespace EnglishTutor.Identity.Application.Commands.Refresh;

/// <summary>
/// Application-layer result for the refresh use case. The Presentation endpoint
/// (<c>AuthEndpoints</c>) maps this to <c>RefreshResponse</c> (Presentation DTO)
/// at the HTTP boundary. Application does NOT reference Presentation.
/// </summary>
public sealed record RefreshResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
