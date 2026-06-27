using Microsoft.AspNetCore.Http;
using System;

namespace EnglishTutor.Identity.Presentation.Auth;

public static class RefreshTokenCookieHelper
{
    public static void SetRefreshTokenCookie(
        HttpResponse response,
        string rawRefreshToken,
        DateTimeOffset expiresAtUtc,
        AuthCookieOptions options)
    {
        response.Cookies.Append(
            options.RefreshTokenCookieName,
            rawRefreshToken,
            BuildCookieOptions(options, expiresAtUtc));
    }

    public static void ClearRefreshTokenCookie(HttpResponse response, AuthCookieOptions options)
    {
        // Match name/path/attributes used when the cookie was set so the
        // browser actually removes it (Batch R1, H-02).
        response.Cookies.Delete(
            options.RefreshTokenCookieName,
            BuildCookieOptions(options, expiresAtUtc: null));
    }

    // __Host- cookies require Secure, Path=/, and NO Domain. We never set
    // Domain here, which keeps the cookie host-locked.
    private static CookieOptions BuildCookieOptions(AuthCookieOptions options, DateTimeOffset? expiresAtUtc)
    {
        var cookie = new CookieOptions
        {
            HttpOnly = options.HttpOnly,
            Secure = options.Secure,
            SameSite = options.SameSite,
            Path = options.RefreshTokenPath,
            IsEssential = true
        };

        if (expiresAtUtc.HasValue)
        {
            cookie.Expires = expiresAtUtc.Value;
        }

        return cookie;
    }
}
