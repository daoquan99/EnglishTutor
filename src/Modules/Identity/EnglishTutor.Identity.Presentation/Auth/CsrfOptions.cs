namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>
/// Options for the double-submit CSRF defense used on cookie-backed auth
/// mutation endpoints (refresh, logout, logout-all, session revoke).
/// </summary>
/// <remarks>
/// The CSRF cookie uses the <c>__Host-</c> prefix so it is host-locked
/// (Secure, no Domain, Path=/), which prevents a sibling/subdomain attacker
/// from injecting a forged double-submit cookie — the classic weakness of the
/// plain double-submit pattern. Unlike the refresh cookie, the CSRF cookie is
/// NOT HttpOnly: the client must read it to echo the value in the request
/// header. See Batch R1, H-03 / <c>security-identity.md</c> CSRF.
/// </remarks>
public sealed class CsrfOptions
{
    public const string SectionName = "Auth:Csrf";

    public string CookieName { get; init; } = "__Host-et_csrf";
    public string HeaderName { get; init; } = "X-CSRF-TOKEN";
    public bool Secure { get; init; } = true;
}
