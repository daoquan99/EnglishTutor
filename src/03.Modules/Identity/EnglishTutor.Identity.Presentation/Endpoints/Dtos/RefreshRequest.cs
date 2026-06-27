namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Public refresh request payload. The plaintext refresh token is sent
/// in the body (HTTPS only) so the client never needs cookie storage.
/// </summary>
public sealed record RefreshRequest(string RefreshToken);
