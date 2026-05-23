using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public class ModuleBoundaryTests
{
    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(BuildingBlocks.Domain.Entity<>).Assembly;

    [Fact]
    public void Domain_Entities_Should_Be_Abstract_Or_Sealed()
    {
        // Entity base classes should be abstract
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(BuildingBlocks.Domain.Entity<>))
            .Should()
            .BeAbstract()
            .GetResult();

        // This test validates that Entity/AggregateRoot bases are abstract
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_Should_Inherit_From_DomainEvent()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceContaining("DomainEvents")
            .Should()
            .Inherit(typeof(BuildingBlocks.Domain.DomainEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_Should_Be_Sealed()
    {
        var result = Types.InAssembly(typeof(BuildingBlocks.SharedKernel.LanguageCode).Assembly)
            .That()
            .Inherit(typeof(BuildingBlocks.Domain.ValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void IntegrationEvents_Should_Inherit_From_IntegrationEvent()
    {
        var result = Types.InAssembly(typeof(BuildingBlocks.EventBus.IntegrationEvent).Assembly)
            .That()
            .ResideInNamespaceContaining("Events")
            .And()
            .AreNotAbstract()
            .Should()
            .Inherit(typeof(BuildingBlocks.EventBus.IntegrationEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
