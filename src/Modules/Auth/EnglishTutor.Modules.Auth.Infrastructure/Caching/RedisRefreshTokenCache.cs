using EnglishTutor.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Modules.Auth.Infrastructure.Caching;

public sealed class RedisRefreshTokenCache(
    IDistributedCache cache,
    ILogger<RedisRefreshTokenCache> logger) : IRefreshTokenCache
{
    public async Task<string?> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetStringAsync(GetKey(sessionId, deviceId), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not read refresh token hash from Redis for session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task StoreTokenHashAsync(
        Guid sessionId,
        string deviceId,
        string tokenHash,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            var ttl = expiresAtUtc - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                return;
            }

            await cache.SetStringAsync(
                GetKey(sessionId, deviceId),
                tokenHash,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not store refresh token hash in Redis for session {SessionId}", sessionId);
        }
    }

    public async Task RemoveTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(GetKey(sessionId, deviceId), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not remove refresh token hash from Redis for session {SessionId}", sessionId);
        }
    }

    private static string GetKey(Guid sessionId, string deviceId) =>
        $"auth:refresh:{sessionId:N}:{deviceId}";
}
