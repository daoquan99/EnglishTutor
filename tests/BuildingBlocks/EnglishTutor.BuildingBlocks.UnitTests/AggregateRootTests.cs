using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class AggregateRootTests
{
    private class TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }

    private class TestAggregate : AggregateRoot
    {
        public TestAggregate() : base() { }

        public void RaiseTestEvent()
        {
            RaiseDomainEvent(new TestDomainEvent());
        }
    }

    [Fact]
    public void New_Aggregate_Should_Have_Empty_Domain_Events()
    {
        // Act
        var aggregate = new TestAggregate();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseDomainEvent_Should_Add_Event_To_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act
        aggregate.RaiseTestEvent();

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents[0].Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_Events()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.RaiseTestEvent();
        aggregate.RaiseTestEvent();

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_Should_Return_ReadOnly_List()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act & Assert
        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }
}
