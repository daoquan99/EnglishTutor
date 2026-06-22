namespace EnglishTutor.Identity.Application.Commands.Login;

/// <summary>
/// Application-layer result for the login use case. The Presentation endpoint
/// (<c>AuthEndpoints</c>) maps this to <c>LoginResponse</c> (Presentation DTO)
/// at the HTTP boundary. Application does NOT reference Presentation.
/// </summary>
public sealed record LoginResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
