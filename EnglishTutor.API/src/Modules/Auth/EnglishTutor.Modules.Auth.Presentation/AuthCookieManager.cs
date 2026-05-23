using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Modules.Auth.Presentation;

internal static class AuthCookieManager
{
    public const string RefreshTokenCookieName = "et_refresh_token";
    public const string SessionIdCookieName = "et_session_id";
    public const string DeviceIdCookieName = "et_device_id";

    public static string GetOrCreateDeviceId(HttpRequest request) =>
        request.Cookies.TryGetValue(DeviceIdCookieName, out var deviceId) && !string.IsNullOrWhiteSpace(deviceId)
            ? deviceId
            : Guid.NewGuid().ToString("N");

    public static bool TryReadRefreshCookies(HttpRequest request, out string refreshToken, out Guid sessionId, out string deviceId)
    {
        refreshToken = request.Cookies.TryGetValue(RefreshTokenCookieName, out var token) ? token : string.Empty;
        var sessionIdValue = request.Cookies.TryGetValue(SessionIdCookieName, out var session) ? session : string.Empty;
        deviceId = request.Cookies.TryGetValue(DeviceIdCookieName, out var device) ? device : string.Empty;
        sessionId = Guid.Empty;

        return !string.IsNullOrWhiteSpace(refreshToken) &&
            Guid.TryParse(sessionIdValue, out sessionId) &&
            !string.IsNullOrWhiteSpace(deviceId);
    }

    public static void SetAuthCookies(HttpResponse response, AuthTokenResponse tokenResponse, AuthCookieSettings settings)
    {
        var options = CreateOptions(tokenResponse.RefreshTokenExpiresAtUtc, settings);
        response.Cookies.Append(RefreshTokenCookieName, tokenResponse.RefreshToken, options);
        response.Cookies.Append(SessionIdCookieName, tokenResponse.SessionId.ToString("N"), options);
        response.Cookies.Append(DeviceIdCookieName, tokenResponse.DeviceId, options);
    }

    public static void ClearAuthCookies(HttpResponse response, AuthCookieSettings settings)
    {
        var options = CreateOptions(DateTimeOffset.UtcNow.AddDays(-1), settings);
        response.Cookies.Delete(RefreshTokenCookieName, options);
        response.Cookies.Delete(SessionIdCookieName, options);
        response.Cookies.Delete(DeviceIdCookieName, options);
    }

    private static CookieOptions CreateOptions(DateTimeOffset expires, AuthCookieSettings settings) =>
        new()
        {
            HttpOnly = true,
            Secure = settings.Secure,
            SameSite = settings.SameSite,
            Path = settings.Path,
            Expires = expires
        };
}

public sealed class AuthCookieSettings
{
    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Lax;
    public string Path { get; init; } = "/api/auth";
}
