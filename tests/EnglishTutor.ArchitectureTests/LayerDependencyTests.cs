using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(BuildingBlocks.Domain.Entity<>).Assembly;

    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(BuildingBlocks.Application.Results.Result).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Outbox()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksOutboxNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_EventBus()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksEventBusNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EventBus()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.BuildingBlocksEventBusNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
