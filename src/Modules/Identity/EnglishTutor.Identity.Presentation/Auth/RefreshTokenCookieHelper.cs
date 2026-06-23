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
            new CookieOptions
            {
                HttpOnly = true,
                Secure = options.Secure,
                SameSite = options.SameSite,
                Path = options.RefreshTokenPath,
                Expires = expiresAtUtc,
                IsEssential = true
            });
    }

    public static void ClearRefreshTokenCookie(HttpResponse response, AuthCookieOptions options)
    {
        response.Cookies.Delete(
            options.RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = options.Secure,
                SameSite = options.SameSite,
                Path = options.RefreshTokenPath
            });
    }
}
