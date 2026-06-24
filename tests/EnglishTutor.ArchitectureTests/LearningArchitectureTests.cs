using FluentAssertions;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Presentation;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public class LearningArchitectureTests
{
    private const string LearningDomainNamespace = "EnglishTutor.Learning.Domain";
    private const string LearningApplicationNamespace = "EnglishTutor.Learning.Application";
    private const string LearningInfrastructureNamespace = "EnglishTutor.Learning.Infrastructure";
    private const string LearningPresentationNamespace = "EnglishTutor.Learning.Presentation";
    private const string LearningContractsNamespace = "EnglishTutor.Learning.Contracts";

    private static readonly string[] FutureModulesNamespaces = new[]
    {
        "EnglishTutor.AiGateway",
        "EnglishTutor.Feedback",
        "EnglishTutor.Practice",
        "EnglishTutor.Progress",
        "EnglishTutor.Quota",
        "EnglishTutor.Realtime"
    };

    private static readonly string[] OtherModulesInternalNamespaces = new[]
    {
        "EnglishTutor.Identity.Domain",
        "EnglishTutor.Identity.Application",
        "EnglishTutor.Identity.Infrastructure",
        "EnglishTutor.Identity.Presentation",
        "EnglishTutor.Audit.Domain",
        "EnglishTutor.Audit.Application",
        "EnglishTutor.Audit.Infrastructure",
        "EnglishTutor.Audit.Presentation"
    };

    [Fact]
    public void Learning_Domain_Should_Not_Depend_On_Application_Infrastructure_Presentation_Or_Other_Modules()
    {
        var result = Types.InAssembly(typeof(Topic).Assembly)
            .ShouldNot()
            .HaveDependencyOn(LearningApplicationNamespace)
            .And()
            .HaveDependencyOn(LearningInfrastructureNamespace)
            .And()
            .HaveDependencyOn(LearningPresentationNamespace)
            .And()
            .HaveDependencyOn(LearningContractsNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Learning.Domain must be clean and not depend on Application, Infrastructure, Presentation, or Contracts. Failing types: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Learning_Application_Should_Not_Depend_On_Infrastructure_Presentation_Or_Other_Modules_Internal_Layers()
    {
        var types = Types.InAssembly(typeof(ILearningUnitOfWork).Assembly);

        // Disallow depending on Presentation or Infrastructure
        var result = types
            .ShouldNot()
            .HaveDependencyOn(LearningInfrastructureNamespace)
            .And()
            .HaveDependencyOn(LearningPresentationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Learning.Application must not depend on Infrastructure or Presentation. Failing types: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));

        // Disallow depending on other modules' internal layers (Identity or Audit)
        foreach (var otherNs in OtherModulesInternalNamespaces)
        {
            var r = types.ShouldNot().HaveDependencyOn(otherNs).GetResult();
            r.IsSuccessful.Should().BeTrue($"Learning.Application must not depend on internal layer {otherNs}");
        }
    }

    [Fact]
    public void Learning_Contracts_Should_Not_Depend_On_Domain_Application_Infrastructure_Or_Presentation()
    {
        var result = Types.InAssembly(typeof(TopicCreatedIntegrationEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOn(LearningDomainNamespace)
            .And()
            .HaveDependencyOn(LearningApplicationNamespace)
            .And()
            .HaveDependencyOn(LearningInfrastructureNamespace)
            .And()
            .HaveDependencyOn(LearningPresentationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Learning.Contracts must not depend on Domain, Application, Infrastructure, or Presentation.");
    }

    [Fact]
    public void Learning_Contracts_Events_Should_Inherit_IntegrationEvent()
    {
        var types = Types.InAssembly(typeof(TopicCreatedIntegrationEvent).Assembly)
            .That()
            .ResideInNamespaceStartingWith($"{LearningContractsNamespace}.Events")
            .And()
            .AreClasses()
            .GetTypes();

        foreach (var type in types)
        {
            if (type.Name.EndsWith("IntegrationEvent"))
            {
                type.Should().BeDerivedFrom<IntegrationEvent>($"{type.FullName} must inherit from IntegrationEvent.");
            }
        }
    }

    [Fact]
    public void Learning_Presentation_Should_Not_Depend_On_Domain_Or_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LearningPresentationServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOn(LearningDomainNamespace)
            .And()
            .HaveDependencyOn(LearningInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Learning.Presentation must not depend directly on Domain or Infrastructure.");
    }

    [Fact]
    public void Learning_Should_Not_Reference_Future_Modules()
    {
        var learningAssemblies = new[]
        {
            typeof(Topic).Assembly,
            typeof(ILearningUnitOfWork).Assembly,
            typeof(LearningDbContext).Assembly,
            typeof(LearningPresentationServiceCollectionExtensions).Assembly,
            typeof(TopicCreatedIntegrationEvent).Assembly
        };

        foreach (var assembly in learningAssemblies)
        {
            foreach (var futureNs in FutureModulesNamespaces)
            {
                var result = Types.InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOn(futureNs)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"Assembly {assembly.GetName().Name} must not reference future module {futureNs}.");
            }
        }
    }

    [Fact]
    public void Learning_Domain_Types_Should_Live_Under_Aggregates_Folders()
    {
        var assembly = typeof(Topic).Assembly;

        var offenders = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceStartingWith(LearningDomainNamespace)
            .And()
            .DoNotResideInNamespaceStartingWith($"{LearningDomainNamespace}.Aggregates.")
            .And()
            .DoNotResideInNamespace($"{LearningDomainNamespace}.Shared")
            .And()
            .DoNotResideInNamespaceStartingWith($"{LearningDomainNamespace}.Shared.")
            .GetTypes()
            .Where(t => !IsCompilerGeneratedOrFramework(t))
            .Select(t => t.FullName ?? t.Name)
            .ToList();

        offenders.Should().BeEmpty("All Learning domain types must live under aggregates or shared folders. Found offenders: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Learning_Domain_Should_Have_Exactly_Five_Aggregate_Folders()
    {
        var assembly = typeof(Topic).Assembly;

        var aggregateNamespaces = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceStartingWith($"{LearningDomainNamespace}.Aggregates.")
            .GetTypes()
            .Where(t => !IsCompilerGeneratedOrFramework(t) && t.FullName != null)
            .Select(t => t.FullName!)
            .ToList();

        aggregateNamespaces.Should().NotBeEmpty("Learning.Domain must contain aggregates.");

        var distinctTopLevelAggregates = aggregateNamespaces
            .Select(fullName =>
            {
                var afterAggregates = fullName.Substring($"{LearningDomainNamespace}.Aggregates.".Length);
                var dot = afterAggregates.IndexOf('.');
                return dot >= 0 ? afterAggregates.Substring(0, dot) : afterAggregates;
            })
            .Distinct()
            .ToList();

        distinctTopLevelAggregates.Should().HaveCount(5, "Learning.Domain must have exactly five aggregate folders: Topics, ModeDefinitions, Scenarios, TopicVocabularies, and TopicPhrases. Found: " + string.Join(", ", distinctTopLevelAggregates));
        distinctTopLevelAggregates.Should().Contain(new[] { "Topics", "ModeDefinitions", "Scenarios", "TopicVocabularies", "TopicPhrases" });
    }

    [Fact]
    public void Learning_Domain_And_Application_Should_Not_Depend_On_Npgsql_Or_Dapper()
    {
        var assemblies = new[]
        {
            typeof(Topic).Assembly,
            typeof(ILearningUnitOfWork).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Npgsql")
                .And()
                .HaveDependencyOn("Dapper")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Assembly {assembly.GetName().Name} must not reference Npgsql or Dapper.");
        }
    }

    private static bool IsCompilerGeneratedOrFramework(Type t)
    {
        if (t.FullName == null) return true;
        if (t.Name.StartsWith("<", StringComparison.Ordinal)) return true;
        var hasCompilerGenerated = t.GetCustomAttributes(false)
            .Any(a => a.GetType().FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        return hasCompilerGenerated;
    }
}
