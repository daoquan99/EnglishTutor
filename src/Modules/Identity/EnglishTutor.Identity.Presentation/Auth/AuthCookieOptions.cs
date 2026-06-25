using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>
/// Options controlling the secure refresh-token cookie.
/// </summary>
/// <remarks>
/// The cookie keeps the <c>__Host-</c> prefix, which the browser only accepts
/// when the cookie is <c>Secure</c>, has <b>no</b> <c>Domain</c> attribute, and
/// uses <c>Path=/</c>. Endpoint scoping is therefore NOT done via cookie path
/// (it is enforced by authentication + CSRF instead). See Batch R1, H-02 and
/// <c>security-identity.md</c>.
/// </remarks>
public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth:Cookies";

    public string RefreshTokenCookieName { get; init; } = "__Host-et_refresh";

    // A __Host- prefixed cookie MUST use Path=/ (and Secure, no Domain).
    public string RefreshTokenPath { get; init; } = "/";
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;
    public bool Secure { get; init; } = true;
    public bool HttpOnly { get; init; } = true;
}
