using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>Outcome of validating a double-submit CSRF token.</summary>
public enum CsrfValidationResult
{
    Valid = 0,
    Missing = 1,
    Invalid = 2
}

/// <summary>
/// Double-submit CSRF token issue + validation. The token is a random,
/// opaque value placed both in the <c>__Host-et_csrf</c> cookie and echoed by
/// the client in the <c>X-CSRF-TOKEN</c> header; the server requires both to
/// be present and equal (constant-time). Header presence alone is NOT accepted
/// (Batch R1, H-03 replaces the old header-presence check).
/// </summary>
public static class CsrfProtection
{
    /// <summary>
    /// Generates a fresh CSRF token, sets it on the response as the
    /// double-submit cookie, and returns the raw value for the caller to echo
    /// in the header. Used by the <c>GET /api/auth/csrf</c> bootstrap endpoint.
    /// </summary>
    public static string IssueToken(HttpResponse response, CsrfOptions options)
    {
        var token = GenerateToken();
        response.Cookies.Append(
            options.CookieName,
            token,
            new CookieOptions
            {
                // Double-submit: must be readable by client script to echo in
                // the header, so HttpOnly is intentionally false. __Host- +
                // Secure + Path=/ + no Domain keeps it host-locked.
                HttpOnly = false,
                Secure = options.Secure,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                IsEssential = true
            });
        return token;
    }

    public static CsrfValidationResult Validate(HttpRequest request, CsrfOptions options)
    {
        var cookieToken = request.Cookies[options.CookieName];
        var headerToken = request.Headers[options.HeaderName].ToString();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken))
        {
            return CsrfValidationResult.Missing;
        }

        var cookieBytes = Encoding.UTF8.GetBytes(cookieToken);
        var headerBytes = Encoding.UTF8.GetBytes(headerToken);

        // CryptographicOperations.FixedTimeEquals returns false for unequal
        // lengths and compares in constant time otherwise.
        return CryptographicOperations.FixedTimeEquals(cookieBytes, headerBytes)
            ? CsrfValidationResult.Valid
            : CsrfValidationResult.Invalid;
    }

    private static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
