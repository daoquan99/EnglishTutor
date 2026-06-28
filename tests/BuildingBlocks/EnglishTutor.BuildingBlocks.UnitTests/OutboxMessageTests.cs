using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class OutboxMessageTests
{
    [Fact]
    public void Create_Should_Copy_Envelope_And_Routing_Metadata()
    {
        var eventId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var occurredAtUtc = DateTime.UtcNow;
        var createdAtUtc = DateTimeOffset.UtcNow;
        var integrationEvent = new TestIntegrationEvent
        {
            EventId = eventId,
            OccurredAtUtc = occurredAtUtc,
            CorrelationId = correlationId,
            CausationId = causationId,
            SchemaVersion = "2.0"
        };
        var descriptor = new MessageContractDescriptor(
            typeof(TestIntegrationEvent),
            "test.event.v2",
            "english.integration",
            "test.event.v2");

        var message = OutboxMessage.Create(
            integrationEvent,
            descriptor,
            "{\"value\":1}",
            createdAtUtc);

        message.Id.Should().Be(eventId);
        message.ContractName.Should().Be("test.event.v2");
        message.SchemaVersion.Should().Be("2.0");
        message.ExchangeName.Should().Be("english.integration");
        message.RoutingKey.Should().Be("test.event.v2");
        message.OccurredAtUtc.Should().Be(new DateTimeOffset(occurredAtUtc, TimeSpan.Zero));
        message.PayloadJson.Should().Be("{\"value\":1}");
        message.CorrelationId.Should().Be(correlationId);
        message.CausationId.Should().Be(causationId);
        message.Status.Should().Be(OutboxMessageStatus.Pending);
        message.AttemptCount.Should().Be(0);
        message.NextAttemptAtUtc.Should().Be(createdAtUtc);
        message.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void MarkPublished_Should_Reject_Message_Not_Owned_By_Lease()
    {
        var message = CreateMessage();

        var act = () => message.MarkPublished(Guid.NewGuid(), DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkFailed_Should_Reject_Message_Not_Owned_By_Lease()
    {
        var message = CreateMessage();

        var act = () => message.MarkFailed(
            Guid.NewGuid(),
            "broker_unavailable",
            "RabbitMQ unavailable",
            DateTimeOffset.UtcNow.AddSeconds(5),
            5);

        act.Should().Throw<InvalidOperationException>();
    }

    private static OutboxMessage CreateMessage() => OutboxMessage.Create(
        new TestIntegrationEvent(),
        new MessageContractDescriptor(
            typeof(TestIntegrationEvent),
            "test.event.v1",
            "english.integration",
            "test.event.v1"),
        "{}",
        DateTimeOffset.UtcNow);

    private sealed record TestIntegrationEvent : IntegrationEvent;
}
