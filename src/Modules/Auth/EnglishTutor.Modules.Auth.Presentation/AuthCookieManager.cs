using EnglishTutor.Modules.Auth.Application.DTOs;
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

    public static void SetAuthCookies(HttpResponse response, AuthTokenResponse tokenResponse)
    {
        var options = CreateOptions(tokenResponse.RefreshTokenExpiresAtUtc);
        response.Cookies.Append(RefreshTokenCookieName, tokenResponse.RefreshToken, options);
        response.Cookies.Append(SessionIdCookieName, tokenResponse.SessionId.ToString("N"), options);
        response.Cookies.Append(DeviceIdCookieName, tokenResponse.DeviceId, options);
    }

    public static void ClearAuthCookies(HttpResponse response)
    {
        var options = CreateOptions(DateTimeOffset.UtcNow.AddDays(-1));
        response.Cookies.Delete(RefreshTokenCookieName, options);
        response.Cookies.Delete(SessionIdCookieName, options);
        response.Cookies.Delete(DeviceIdCookieName, options);
    }

    private static CookieOptions CreateOptions(DateTimeOffset expires) =>
        new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = expires
        };
}
