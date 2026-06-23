using FluentAssertions;
using NetArchTest.Rules;

namespace EnglishTutor.ArchitectureTests;

public class ModuleDependencyTests
{
    private const string BuildingBlocksDomainNamespace = "EnglishTutor.BuildingBlocks.Domain";
    private const string BuildingBlocksApplicationNamespace = "EnglishTutor.BuildingBlocks.Application";
    private const string BuildingBlocksInfrastructureNamespace = "EnglishTutor.BuildingBlocks.Infrastructure";
    private const string BuildingBlocksContractsNamespace = "EnglishTutor.BuildingBlocks.Contracts";

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.BuildingBlocks.Domain.Entities.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.BuildingBlocks.Domain.Entities.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.BuildingBlocks.Application.Commands.ICommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.BuildingBlocks.Contracts.Events.IntegrationEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.BuildingBlocks.Contracts.Events.IntegrationEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Identity_Domain_Should_Not_Depend_On_Other_Modules()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.Identity.Domain.IIdentityDomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("EnglishTutor.Learning")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Identity_Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.Identity.Application.IIdentityApplicationMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("EnglishTutor.Identity.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void BuildingBlocks_Should_Not_Reference_ModuleDbContexts()
    {
        var buildingBlocksAssemblies = new[]
        {
            typeof(EnglishTutor.BuildingBlocks.Domain.Entities.Entity).Assembly,
            typeof(EnglishTutor.BuildingBlocks.Application.Commands.ICommand).Assembly,
            typeof(EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks.StartupReadinessProbe).Assembly
        };

        foreach (var assembly in buildingBlocksAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("EnglishTutor.Identity.Infrastructure")
                .And()
                .HaveDependencyOn("EnglishTutor.Audit.Infrastructure")
                .And()
                .HaveDependencyOn("IdentityDbContext")
                .And()
                .HaveDependencyOn("AuditDbContext")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"BuildingBlocks assembly {assembly.GetName().Name} must not reference Identity or Audit DbContexts/Infrastructure.");
        }
    }
}
