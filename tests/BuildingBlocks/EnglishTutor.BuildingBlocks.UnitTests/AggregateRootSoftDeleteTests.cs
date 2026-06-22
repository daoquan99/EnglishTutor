using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Entities;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class AggregateRootSoftDeleteTests
{
    private sealed class TestAggregate : AggregateRoot
    {
        public TestAggregate() : base() { }
        public TestAggregate(Guid id) : base(id) { }

        public void Touch(string _)
        {
            RaiseDomainEvent(new TestDomainEvent());
        }
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }

    [Fact]
    public void New_Aggregate_IsDeleted_Should_Be_False()
    {
        var agg = new TestAggregate();

        agg.IsDeleted.Should().BeFalse();
        agg.DeletedAtUtc.Should().BeNull();
        agg.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_Should_Set_All_Delete_Fields_And_Raise_Event()
    {
        var agg = new TestAggregate();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        agg.MarkDeleted(userId, now);

        agg.IsDeleted.Should().BeTrue();
        agg.DeletedAtUtc.Should().Be(now);
        agg.DeletedByUserId.Should().Be(userId);

        agg.DomainEvents.Should().ContainSingle(e =>
            e is AggregateDeletedDomainEvent &&
            ((AggregateDeletedDomainEvent)e).AggregateId == agg.Id &&
            ((AggregateDeletedDomainEvent)e).DeletedByUserId == userId &&
            ((AggregateDeletedDomainEvent)e).DeletedAtUtc == now);
    }

    [Fact]
    public void MarkDeleted_Should_Be_Idempotent()
    {
        var agg = new TestAggregate();
        agg.MarkDeleted(Guid.NewGuid(), DateTime.UtcNow);
        agg.MarkDeleted(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));

        agg.DomainEvents.Count(e => e is AggregateDeletedDomainEvent).Should().Be(1);
    }

    [Fact]
    public void Undelete_Should_Clear_All_Delete_Fields()
    {
        var agg = new TestAggregate();
        var userId = Guid.NewGuid();
        agg.MarkDeleted(userId, DateTime.UtcNow);

        agg.Undelete();

        agg.IsDeleted.Should().BeFalse();
        agg.DeletedAtUtc.Should().BeNull();
        agg.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_Should_Accept_Null_UserId_For_System_Operations()
    {
        var agg = new TestAggregate();
        agg.MarkDeleted(null, DateTime.UtcNow);

        agg.IsDeleted.Should().BeTrue();
        agg.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Audit_Fields_And_SoftDelete_Fields_Are_Independent()
    {
        // Soft-delete does not auto-stamp created/updated — interceptor owns
        // those. This documents the separation of concerns.
        var agg = new TestAggregate();
        agg.MarkDeleted(Guid.NewGuid(), DateTime.UtcNow);

        agg.CreatedAtUtc.Should().Be(default);
        agg.UpdatedAtUtc.Should().Be(default);
    }
}
