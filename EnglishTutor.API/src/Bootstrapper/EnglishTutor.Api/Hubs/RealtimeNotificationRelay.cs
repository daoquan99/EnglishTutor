using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EnglishTutor.Api.Hubs;

public sealed class RealtimeNotificationRelay(
    IConnectionMultiplexer redis,
    IHubContext<NotificationHub> hubContext,
    ILogger<RealtimeNotificationRelay> logger) : BackgroundService
{
    private const string Channel = "notifications:realtime";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(Channel), async (_, message) =>
        {
            try
            {
                using var doc = JsonDocument.Parse((string)message!);
                var root = doc.RootElement;

                var userId = root.GetProperty("userId").GetGuid();
                var payload = root.GetProperty("payload");

                await hubContext.Clients
                    .Group($"user:{userId}")
                    .SendAsync("ReceiveNotification", JsonSerializer.Serialize(payload), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to relay realtime notification");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
