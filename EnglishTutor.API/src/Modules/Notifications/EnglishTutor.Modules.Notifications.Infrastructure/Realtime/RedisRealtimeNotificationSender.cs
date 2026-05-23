using System.Text.Json;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Realtime;

public sealed class RedisRealtimeNotificationSender(
    IConnectionMultiplexer redis,
    ILogger<RedisRealtimeNotificationSender> logger) : IRealtimeNotificationSender
{
    private const string Channel = "notifications:realtime";

    public async Task SendNotificationAsync(Guid userId, NotificationPushPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(new
            {
                userId,
                payload
            });

            var subscriber = redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(Channel), message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish realtime notification for user {UserId}", userId);
        }
    }
}
