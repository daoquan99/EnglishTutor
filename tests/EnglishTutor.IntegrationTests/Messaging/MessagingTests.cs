using System.Text.Json;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.IntegrationTests.Messaging.Consumers;
using EnglishTutor.IntegrationTests.Messaging.Events;
using FluentAssertions;

namespace EnglishTutor.IntegrationTests.Messaging;

public class MessagingTests
{
    [Fact]
    public async Task Native_Handler_Should_Deserialize_And_Dispatch_Event()
    {
        var integrationEvent = new MessagingTestEvent("Hello RabbitMQ");
        var context = new MessageDeliveryContext(
            integrationEvent.EventId,
            "tests.messaging.v1",
            integrationEvent.SchemaVersion,
            integrationEvent.CorrelationId,
            integrationEvent.CausationId,
            0,
            DateTimeOffset.UtcNow);
        var consumer = new MessagingTestConsumer();
        MessagingTestConsumer.ConsumedEvents.Clear();

        await ((IRabbitMqMessageHandler)consumer).HandleAsync(
            JsonSerializer.SerializeToUtf8Bytes(integrationEvent),
            context,
            CancellationToken.None);

        MessagingTestConsumer.ConsumedEvents.Should().ContainSingle();
        MessagingTestConsumer.ConsumedEvents[0].Value.Should().Be("Hello RabbitMQ");
    }

    [Fact]
    public async Task Native_Handler_Should_Reject_Invalid_Json()
    {
        var consumer = new MessagingTestConsumer();
        var context = new MessageDeliveryContext(
            Guid.NewGuid(),
            "tests.messaging.v1",
            "1.0",
            null,
            null,
            0,
            DateTimeOffset.UtcNow);

        var act = () => ((IRabbitMqMessageHandler)consumer).HandleAsync(
            "not-json"u8.ToArray(),
            context,
            CancellationToken.None);

        await act.Should().ThrowAsync<JsonException>();
    }
}
