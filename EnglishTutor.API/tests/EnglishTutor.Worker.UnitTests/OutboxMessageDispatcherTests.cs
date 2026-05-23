using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Worker.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EnglishTutor.Worker.UnitTests;

public sealed class OutboxMessageDispatcherTests
{
    private static readonly DateTime BaseUtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly JsonSerializerService _serializer = new();

    [Fact]
    public async Task DispatchAsync_Marks_Message_Processed_When_EventBus_Succeeds()
    {
        var eventBus = new CapturingEventBus();
        var dispatcher = CreateDispatcher(eventBus);
        var message = CreateMessage(new TestIntegrationEvent(Guid.NewGuid()));

        var deadLetter = await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.Null(deadLetter);
        Assert.Equal(OutboxMessageStatus.Processed, message.Status);
        Assert.NotNull(message.ProcessedAtUtc);
        Assert.Null(message.LockedBy);
        Assert.Null(message.LockedUntilUtc);
        Assert.Equal(1, eventBus.DispatchCount);
    }

    [Fact]
    public async Task DispatchAsync_Schedules_Retry_When_Handler_Fails_Before_MaxRetry()
    {
        var dispatcher = CreateDispatcher(new CapturingEventBus(shouldThrow: true));
        var message = CreateMessage(new TestIntegrationEvent(Guid.NewGuid()));
        message.MaxRetryCount = 3;

        var deadLetter = await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.Null(deadLetter);
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.NotNull(message.NextRetryAtUtc);
        Assert.NotNull(message.LastError);
    }

    [Fact]
    public async Task DispatchAsync_Returns_DeadLetter_When_MaxRetry_Is_Reached()
    {
        var dispatcher = CreateDispatcher(new CapturingEventBus(shouldThrow: true));
        var message = CreateMessage(new TestIntegrationEvent(Guid.NewGuid()));
        message.MaxRetryCount = 1;

        var deadLetter = await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.NotNull(deadLetter);
        Assert.Equal(OutboxMessageStatus.DeadLettered, message.Status);
        Assert.Null(message.NextRetryAtUtc);
        Assert.Equal(message.EventId, deadLetter.EventId);
        Assert.Equal(message.EventType, deadLetter.EventType);
        Assert.Equal(message.Payload, deadLetter.Payload);
        Assert.Equal(message.SourceModule, deadLetter.SourceModule);
    }

    private OutboxMessageDispatcher CreateDispatcher(IEventBus eventBus) =>
        new(
            eventBus,
            _serializer,
            NullLogger<OutboxMessageDispatcher>.Instance,
            new FakeDateTimeProvider(),
            Options.Create(new OutboxOptions()));

    private OutboxMessage CreateMessage(TestIntegrationEvent integrationEvent) =>
        new()
        {
            EventId = integrationEvent.EventId,
            EventType = OutboxMessageFactory.ResolveStableTypeName(integrationEvent.GetType()),
            Payload = _serializer.Serialize(integrationEvent),
            SourceModule = "test"
        };

    private sealed class CapturingEventBus(bool shouldThrow = false) : IEventBus
    {
        public int DispatchCount { get; private set; }

        public Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default)
        {
            DispatchCount++;

            if (shouldThrow)
            {
                throw new InvalidOperationException("Handler failed.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => OutboxMessageDispatcherTests.BaseUtcNow;
    }
}

public sealed record TestIntegrationEvent(Guid UserId) : IntegrationEvent;

