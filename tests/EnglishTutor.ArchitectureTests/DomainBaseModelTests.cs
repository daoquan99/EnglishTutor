using EnglishTutor.BuildingBlocks.Domain;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public class DomainBaseModelTests
{
    [Fact]
    public void Entity_Should_Be_Auditable()
    {
        typeof(Entity<Guid>).Should().BeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void AggregateRoot_Should_Be_SoftDeletable_And_Hold_DomainEvents()
    {
        typeof(AggregateRoot<Guid>).Should().BeAssignableTo<ISoftDeletable>();
        typeof(AggregateRoot<Guid>).Should().BeAssignableTo<IDomainEventHolder>();
    }
}
