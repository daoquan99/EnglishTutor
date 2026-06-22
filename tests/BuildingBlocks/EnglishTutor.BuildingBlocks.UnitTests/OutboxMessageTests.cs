using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class OutboxMessageTests
{
    // ===== Fix #1: ModuleName required =====

    [Fact]
    public void Constructor_Should_Require_ModuleName()
    {
        // Act
        var act = () => new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow,
            moduleName: "",
            type: "TestEvent",
            payloadJson: "{}");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("moduleName");
    }

    [Fact]
    public void Constructor_Should_Require_Type()
    {
        var act = () => new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow,
            moduleName: "Practice",
            type: "",
            payloadJson: "{}");

        act.Should().Throw<ArgumentException>().WithParameterName("type");
    }

    [Fact]
    public void Constructor_Should_Set_ModuleName()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow,
            "Practice", "PracticeEndedIntegrationEvent", "{}");

        message.ModuleName.Should().Be("Practice");
        message.Type.Should().Be("PracticeEndedIntegrationEvent");
    }

    // ===== Fix #2: CorrelationId / CausationId are now Guid? (aligned with IntegrationEvent) =====

    [Fact]
    public void Constructor_Should_Set_All_Required_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        // Act
        var message = new OutboxMessage(
            id: id,
            occurredAtUtc: occurredAt,
            moduleName: "Practice",
            type: "TestEvent",
            payloadJson: "{\"key\":\"value\"}",
            correlationId: correlationId,
            causationId: causationId);

        // Assert
        message.Id.Should().Be(id);
        message.OccurredAtUtc.Should().Be(occurredAt);
        message.ModuleName.Should().Be("Practice");
        message.Type.Should().Be("TestEvent");
        message.PayloadJson.Should().Be("{\"key\":\"value\"}");
        message.CorrelationId.Should().Be(correlationId);
        message.CausationId.Should().Be(causationId);
        message.RetryCount.Should().Be(0);
        message.ProcessedAtUtc.Should().BeNull();
        message.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Correlation_And_Causation()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow, "Practice", "T", "{}");

        message.CorrelationId.Should().BeNull();
        message.CausationId.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_Type_Should_Match_IntegrationEvent_Guid()
    {
        // Regression: outbox must align with IntegrationEvent.CorrelationId (Guid?).
        var guid = Guid.NewGuid();
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow, "Practice", "T", "{}",
            correlationId: guid);

        message.CorrelationId.Should().Be(guid);
        message.CorrelationId.Should().NotBeNull();
    }

    [Fact]
    public void MarkProcessed_Should_Set_ProcessedAtUtc_And_Clear_Error()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow, "Practice", "T", "{}");
        // Seed a failed state via the public domain API (private setters forbid
        // direct assignment).
        message.MarkFailed("previous error");

        var processedAt = DateTime.UtcNow.AddSeconds(1);
        message.MarkProcessed(processedAt);

        message.ProcessedAtUtc.Should().Be(processedAt);
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkFailed_Should_Set_Error_And_Increment_RetryCount()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow, "Practice", "T", "{}");

        message.MarkFailed("first error");
        message.Error.Should().Be("first error");
        message.RetryCount.Should().Be(1);

        message.MarkFailed("second error");
        message.Error.Should().Be("second error");
        message.RetryCount.Should().Be(2);
    }

    [Fact]
    public void OutboxMessage_Should_Implement_IOutboxMessage()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), DateTime.UtcNow, "Practice", "T", "{}");

        message.Should().BeAssignableTo<IOutboxMessage>();
    }
}
