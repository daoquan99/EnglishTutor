namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IRefreshTokenCache
{
    Task<RefreshTokenCacheReadResult> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken);

    Task<bool> StoreTokenHashAsync(
        Guid sessionId,
        string deviceId,
        string tokenHash,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task RemoveTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken);
}

public sealed record RefreshTokenCacheReadResult(bool IsAvailable, string? TokenHash)
{
    public static RefreshTokenCacheReadResult Available(string? tokenHash) => new(true, tokenHash);

    public static RefreshTokenCacheReadResult Unavailable() => new(false, null);
}
