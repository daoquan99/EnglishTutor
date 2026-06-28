extern alias WorkerAssembly;
using System;
using System.Linq;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public class MessagingArchitectureTests
{
    private const string IdentityDomainNamespace = "EnglishTutor.Identity.Domain";
    private const string IdentityApplicationNamespace = "EnglishTutor.Identity.Application";
    private const string AuditDomainNamespace = "EnglishTutor.Audit.Domain";
    private const string AuditApplicationNamespace = "EnglishTutor.Audit.Application";

    [Fact]
    public void Domain_Assemblies_Should_Not_Reference_RabbitMqClient()
    {
        var assemblies = new[]
        {
            typeof(EnglishTutor.Identity.Domain.IIdentityDomainMarker).Assembly,
            typeof(EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent).Assembly,
            typeof(EnglishTutor.Learning.Domain.Aggregates.Topics.Topic).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("RabbitMQ.Client")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Domain assembly {assembly.GetName().Name} must not reference RabbitMQ.Client.");
        }
    }

    [Fact]
    public void Application_Assemblies_Should_Not_Reference_RabbitMqClient()
    {
        var assemblies = new[]
        {
            typeof(EnglishTutor.Identity.Application.IIdentityApplicationMarker).Assembly,
            typeof(EnglishTutor.Audit.Application.Commands.RecordSecurityEvent.RecordSecurityEventCommand).Assembly,
            typeof(EnglishTutor.Learning.Application.Abstractions.Persistence.ILearningUnitOfWork).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("RabbitMQ.Client")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Application assembly {assembly.GetName().Name} must not reference RabbitMQ.Client.");
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
            typeof(EnglishTutor.Learning.Domain.Aggregates.Topics.Topic).Assembly,
            typeof(EnglishTutor.Learning.Application.Abstractions.Persistence.ILearningUnitOfWork).Assembly,
            typeof(EnglishTutor.Learning.Infrastructure.Persistence.LearningDbContext).Assembly,
            typeof(EnglishTutor.Learning.Presentation.LearningPresentationServiceCollectionExtensions).Assembly,
            typeof(EnglishTutor.Learning.Contracts.Events.TopicCreatedIntegrationEvent).Assembly,
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
            .ImplementInterface(typeof(IRabbitMqMessageHandler))
            .GetTypes()
            .ToList();

        consumerTypes.Should().BeEmpty(
            "API must not host RabbitMQ message handlers. All consumers are composed by the Worker. " +
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
            .ImplementInterface(typeof(IRabbitMqMessageHandler))
            .GetTypes()
            .ToList();

        consumerTypes.Should().BeEmpty(
            "Worker host must compose module handlers without defining transport handlers itself. " +
            $"Found offending types: {string.Join(", ", consumerTypes.Select(t => t.FullName))}");
    }

    [Fact]
    public void UnitOfWork_Should_Not_Have_Forbidden_Dependencies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.StartsWith("EnglishTutor.", StringComparison.Ordinal))
            .ToList();

        foreach (var assembly in assemblies)
        {
            var isInfrastructure = assembly.FullName!.Contains(".Infrastructure");
            if (!isInfrastructure)
            {
                continue;
            }

            var unitOfWorkTypes = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("UnitOfWork")
                .GetTypes()
                .ToList();

            foreach (var type in unitOfWorkTypes)
            {
                var httpContextAccessorDependency = Types.InAssembly(assembly)
                    .That()
                    .HaveName(type.Name)
                    .ShouldNot()
                    .HaveDependencyOn("Microsoft.AspNetCore.Http.IHttpContextAccessor")
                    .GetResult();

                httpContextAccessorDependency.IsSuccessful.Should().BeTrue($"{type.Name} must not depend on IHttpContextAccessor.");

                var moduleName = type.Namespace?.Split('.')[1];
                if (moduleName != null)
                {
                    var forbiddenNamespace = $"EnglishTutor.{moduleName}.Contracts.Events";
                    var contractsEventsDependency = Types.InAssembly(assembly)
                        .That()
                        .HaveName(type.Name)
                        .ShouldNot()
                        .HaveDependencyOn(forbiddenNamespace)
                        .GetResult();

                    contractsEventsDependency.IsSuccessful.Should().BeTrue($"{type.Name} must not depend on {forbiddenNamespace}.");
                }
            }
        }
    }

    [Fact]
    public void Application_Domain_Event_Handlers_Should_Not_Reference_Infrastructure_Types()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.StartsWith("EnglishTutor.", StringComparison.Ordinal))
            .ToList();

        foreach (var assembly in assemblies)
        {
            var isApplication = assembly.FullName!.Contains(".Application");
            if (!isApplication)
            {
                continue;
            }

            // Get types implementing IDomainEventHandler<>
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                .ToList();

            if (!handlerTypes.Any())
            {
                continue;
            }

            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IDomainEventHandler<>))
                .ShouldNot()
                .HaveDependencyOn("RabbitMQ.Client")
                .And()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .And()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Domain event handlers in {assembly.GetName().Name} must not reference RabbitMQ.Client, EF Core, or ASP.NET.");
        }
    }
}
