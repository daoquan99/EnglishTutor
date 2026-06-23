using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Presentation.Auth;

public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth:Cookies";

    public string RefreshTokenCookieName { get; init; } = "__Host-et_refresh";
    public string RefreshTokenPath { get; init; } = "/api/auth/refresh";
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;
    public bool Secure { get; init; } = true;
    public bool HttpOnly { get; init; } = true;
}
