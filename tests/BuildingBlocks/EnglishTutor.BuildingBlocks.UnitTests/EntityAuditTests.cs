using EnglishTutor.BuildingBlocks.Domain.Entities;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class EntityAuditTests
{
    private sealed class TestEntity : Entity
    {
        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }

        // Expose the protected internal stamping helpers for testing.
        public void PublicStampCreated(Guid? userId, DateTime nowUtc) => StampCreated(userId, nowUtc);
        public void PublicStampUpdated(Guid? userId, DateTime nowUtc) => StampUpdated(userId, nowUtc);
    }

    [Fact]
    public void New_Entity_Should_Have_Default_Audit_Fields()
    {
        // Audit fields are stamped by EF Core interceptor — constructor leaves
        // them at default until SaveChanges runs.
        var entity = new TestEntity();

        entity.CreatedAtUtc.Should().Be(default);
        entity.UpdatedAtUtc.Should().Be(default);
        entity.CreatedByUserId.Should().BeNull();
        entity.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void StampCreated_Should_Set_Created_Fields()
    {
        var entity = new TestEntity();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        entity.PublicStampCreated(userId, now);

        entity.CreatedAtUtc.Should().Be(now);
        entity.CreatedByUserId.Should().Be(userId);
        entity.UpdatedAtUtc.Should().Be(default); // updated-at NOT touched
    }

    [Fact]
    public void StampCreated_Should_Be_Idempotent()
    {
        var entity = new TestEntity();
        var firstTime = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        entity.PublicStampCreated(userId, firstTime);
        entity.PublicStampCreated(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));

        // First call wins — we don't overwrite created-at on re-stamp.
        entity.CreatedAtUtc.Should().Be(firstTime);
        entity.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void StampUpdated_Should_Always_Overwrite()
    {
        var entity = new TestEntity();
        var createdUser = Guid.NewGuid();
        var createdTime = DateTime.UtcNow;
        entity.PublicStampCreated(createdUser, createdTime);

        var updatedUser = Guid.NewGuid();
        var updatedTime = createdTime.AddMinutes(5);
        entity.PublicStampUpdated(updatedUser, updatedTime);

        entity.UpdatedAtUtc.Should().Be(updatedTime);
        entity.UpdatedByUserId.Should().Be(updatedUser);
        // Created fields should remain unchanged.
        entity.CreatedAtUtc.Should().Be(createdTime);
        entity.CreatedByUserId.Should().Be(createdUser);
    }

    [Fact]
    public void StampCreated_Should_Accept_Null_UserId()
    {
        // Null userId represents system-initiated operations (seed, background jobs).
        var entity = new TestEntity();
        var now = DateTime.UtcNow;

        entity.PublicStampCreated(null, now);

        entity.CreatedByUserId.Should().BeNull();
        entity.CreatedAtUtc.Should().Be(now);
    }
}
