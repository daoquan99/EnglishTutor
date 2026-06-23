extern alias WorkerAssembly;
using FluentAssertions;
using MassTransit;
using NetArchTest.Rules;
using System.Reflection;

namespace EnglishTutor.ArchitectureTests;

public class MessagingArchitectureTests
{
    private const string IdentityDomainNamespace = "EnglishTutor.Identity.Domain";
    private const string IdentityApplicationNamespace = "EnglishTutor.Identity.Application";
    private const string AuditDomainNamespace = "EnglishTutor.Audit.Domain";
    private const string AuditApplicationNamespace = "EnglishTutor.Audit.Application";

    [Fact]
    public void Domain_Assemblies_Should_Not_Reference_MassTransit()
    {
        var assemblies = new[]
        {
            typeof(EnglishTutor.Identity.Domain.IIdentityDomainMarker).Assembly,
            typeof(EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("MassTransit")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Domain assembly {assembly.GetName().Name} must not reference MassTransit.");
        }
    }

    [Fact]
    public void Application_Assemblies_Should_Not_Reference_MassTransit()
    {
        var assemblies = new[]
        {
            typeof(EnglishTutor.Identity.Application.IIdentityApplicationMarker).Assembly,
            typeof(EnglishTutor.Audit.Application.Commands.RecordSecurityEvent.RecordSecurityEventCommand).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("MassTransit")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Application assembly {assembly.GetName().Name} must not reference MassTransit.");
        }
    }

    [Fact]
    public void Production_Code_Should_Not_Reference_TestEvent()
    {
        var productionAssemblies = new[]
        {
            typeof(EnglishTutor.Identity.Domain.IIdentityDomainMarker).Assembly,
            typeof(EnglishTutor.Identity.Application.IIdentityApplicationMarker).Assembly,
            typeof(EnglishTutor.Identity.Infrastructure.IdentityInfrastructureServiceCollectionExtensions).Assembly,
            typeof(EnglishTutor.Identity.Presentation.IIdentityPresentationMarker).Assembly,
            typeof(EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent).Assembly,
            typeof(EnglishTutor.Audit.Application.Commands.RecordSecurityEvent.RecordSecurityEventCommand).Assembly,
            typeof(EnglishTutor.Audit.Infrastructure.AuditInfrastructureServiceCollectionExtensions).Assembly,
            typeof(EnglishTutor.Audit.Presentation.IAuditPresentationMarker).Assembly,
            typeof(Program).Assembly
        };

        foreach (var assembly in productionAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("EnglishTutor.IntegrationTests.Messaging")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Production assembly {assembly.GetName().Name} must not reference test-only MessagingTestEvent.");
        }
    }

    [Fact]
    public void Integration_Events_Should_Live_In_Contracts_Projects_Only()
    {
        // Load all assemblies in the solution that start with EnglishTutor.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.StartsWith("EnglishTutor.", StringComparison.Ordinal))
            .ToList();

        var offendingEvents = new List<string>();

        foreach (var assembly in assemblies)
        {
            var isTestAssembly = assembly.FullName!.Contains("Tests");
            var isContractsAssembly = assembly.FullName!.Contains(".Contracts");

            if (isTestAssembly || isContractsAssembly)
            {
                continue;
            }

            var integrationEventTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.Name == "IntegrationEvent")
                .Select(t => t.FullName ?? t.Name)
                .ToList();

            if (integrationEventTypes.Any())
            {
                offendingEvents.AddRange(integrationEventTypes);
            }
        }

        offendingEvents.Should().BeEmpty(
            "Integration events must reside in .Contracts projects only. " +
            $"Found offending integration events in non-contract assemblies: {string.Join(", ", offendingEvents)}");
    }

    [Fact]
    public void Api_Should_Not_Host_Production_Consumers()
    {
        var consumerTypes = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ImplementInterface(typeof(IConsumer))
            .GetTypes()
            .ToList();

        consumerTypes.Should().BeEmpty(
            "API must not host any MassTransit consumers. All consumers must be hosted in the Worker or Test projects. " +
            $"Found offending types: {string.Join(", ", consumerTypes.Select(t => t.FullName))}");
    }

    [Fact]
    public void Worker_Should_Not_Reference_TestOnlyMessagingEvents()
    {
        var workerAssembly = typeof(WorkerAssembly::EnglishTutor.Worker.Options.WorkerOptions).Assembly;

        var result = Types.InAssembly(workerAssembly)
            .ShouldNot()
            .HaveDependencyOn("EnglishTutor.IntegrationTests.Messaging")
            .And()
            .HaveDependencyOn("MessagingTestEvent")
            .And()
            .HaveDependencyOn("MessagingTestConsumer")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Worker project must not reference test-only messaging namespaces, events, or consumers.");
    }

    [Fact]
    public void Worker_Should_Not_Implement_DeferredConsumers()
    {
        var workerAssembly = typeof(WorkerAssembly::EnglishTutor.Worker.Options.WorkerOptions).Assembly;

        var consumerTypes = Types.InAssembly(workerAssembly)
            .That()
            .ImplementInterface(typeof(IConsumer))
            .GetTypes()
            .ToList();

        consumerTypes.Should().BeEmpty(
            "Worker project must not contain any concrete MassTransit consumers for Task 12 baseline. " +
            $"Found offending types: {string.Join(", ", consumerTypes.Select(t => t.FullName))}");
    }
}
