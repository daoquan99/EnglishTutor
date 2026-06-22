namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Public login request payload.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
