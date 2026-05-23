using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EnglishTutor.Modules.Auth.Infrastructure.Caching;

public sealed class RedisRefreshTokenCache(
    IDistributedCache cache,
    ILogger<RedisRefreshTokenCache> logger,
    IDateTimeProvider dateTimeProvider) : IRefreshTokenCache
{
    public async Task<RefreshTokenCacheReadResult> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            return RefreshTokenCacheReadResult.Available(
                await cache.GetStringAsync(GetKey(sessionId, deviceId), cancellationToken));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not read refresh token hash from Redis for session {SessionId}", sessionId);
            return RefreshTokenCacheReadResult.Unavailable();
        }
    }

    public async Task<bool> StoreTokenHashAsync(
        Guid sessionId,
        string deviceId,
        string tokenHash,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            var ttl = expiresAtUtc - dateTimeProvider.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                logger.LogWarning("Could not store refresh token hash in Redis for session {SessionId} because the token is already expired.", sessionId);
                return false;
            }

            await cache.SetStringAsync(
                GetKey(sessionId, deviceId),
                tokenHash,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not store refresh token hash in Redis for session {SessionId}", sessionId);
            return false;
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

    private static string GetKey(Guid sessionId, string deviceId)
    {
        var deviceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(deviceId.Trim()))).ToLowerInvariant();
        return $"auth:refresh:{sessionId:N}:{deviceKey}";
    }
}
