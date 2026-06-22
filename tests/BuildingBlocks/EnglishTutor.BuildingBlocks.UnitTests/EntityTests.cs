using EnglishTutor.BuildingBlocks.Domain.Entities;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void New_Entity_Should_Have_New_Guid()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Entity_With_Same_Id_Should_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Assert
        entity1.Should().Be(entity2);
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Entity_With_Different_Id_Should_Not_Be_Equal()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.Should().NotBe(entity2);
    }

    [Fact]
    public void Entity_Should_Not_Be_Equal_To_Null()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equality_Operator_Should_Work_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }
}
