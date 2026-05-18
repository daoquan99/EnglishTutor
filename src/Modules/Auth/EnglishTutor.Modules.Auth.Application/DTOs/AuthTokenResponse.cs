using System.Text.Json.Serialization;

namespace EnglishTutor.Modules.Auth.Application.DTOs;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    [property: JsonIgnore] string RefreshToken,
    [property: JsonIgnore] Guid SessionId,
    [property: JsonIgnore] string DeviceId,
    [property: JsonIgnore] DateTime RefreshTokenExpiresAtUtc);
