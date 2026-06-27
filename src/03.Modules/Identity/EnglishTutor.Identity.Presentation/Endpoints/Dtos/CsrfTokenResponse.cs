namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Response of <c>GET /api/auth/csrf</c>. The token is also set as the
/// <c>__Host-et_csrf</c> double-submit cookie; the client echoes this value in
/// the <c>X-CSRF-TOKEN</c> header on mutation requests (Batch R1, H-03).
/// </summary>
public sealed record CsrfTokenResponse(string Token);
