using FluentAssertions;
using NetArchTest.Rules;

namespace EnglishTutor.ArchitectureTests;

// Architecture tests for the Audit ↔ Identity boundary.
//
// Task 22A explicitly tightened the Audit ↔ Identity relationship:
//   - Audit.Infrastructure must NOT reference Identity.Domain or
//     Identity.Application (Audit receives Identity-domain events
//     only via Domain contracts — IDs and reason codes — never via
//     direct Identity.Application / Identity.Domain types).
//   - Identity.Application must NOT reference Audit (Identity publishes
//     security events; it does not depend on Audit).
//   - Identity.Infrastructure may reference Audit.Contracts only
//     (the recorder interface), NOT Audit.Application /
//     Audit.Infrastructure / Audit.Domain.
//   - The Audit domain must be aggregate-first: every Audit
//     domain type lives under
//     src/Modules/Audit/EnglishTutor.Audit.Domain/Aggregates/SecurityEvents/
//     — never at the Domain project root outside Aggregates/.
//
// These tests are structural / architectural — they execute via
// NetArchTest on the referenced module assemblies. They are the
// hard gate per .agents/rules/42-testing-and-quality-gates.md § 7.
public class AuditIdentityBoundaryTests
{
    private const string IdentityDomainNamespace = "EnglishTutor.Identity.Domain";
    private const string IdentityApplicationNamespace = "EnglishTutor.Identity.Application";
    private const string IdentityInfrastructureNamespace = "EnglishTutor.Identity.Infrastructure";
    private const string AuditDomainNamespace = "EnglishTutor.Audit.Domain";
    private const string AuditApplicationNamespace = "EnglishTutor.Audit.Application";
    private const string AuditInfrastructureNamespace = "EnglishTutor.Audit.Infrastructure";
    private const string AuditContractsNamespace = "EnglishTutor.Audit.Contracts";

    // ---- Audit.Infrastructure boundary ----

    [Fact]
    public void Audit_Infrastructure_Should_Not_Depend_On_Identity_Domain()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.Audit.Infrastructure.AuditInfrastructureServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOn(IdentityDomainNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Audit.Infrastructure must NOT reference Identity.Domain. Audit receives " +
            "Identity ids / reason codes through the SecurityEvent payload — never " +
            "through Identity.Domain types. Found offending types: " +
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Audit_Infrastructure_Should_Not_Depend_On_Identity_Application()
    {
        var result = Types.InAssembly(typeof(EnglishTutor.Audit.Infrastructure.AuditInfrastructureServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOn(IdentityApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Audit.Infrastructure must NOT reference Identity.Application. The audit " +
            "pipeline must remain Identity-agnostic. Found offending types: " +
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    // ---- Identity boundary ----

    [Fact]
    public void Identity_Application_Should_Not_Depend_On_Audit()
    {
        // Identity.Application must NOT reference any Audit assembly
        // (Domain, Application, Infrastructure, or Contracts).
        // Identity publishes security events through its own
        // domain events; Audit is consumed downstream via the
        // Identity.Infrastructure.RefreshTokenReuseAuditHandler.
        var identityApplicationAssembly =
            typeof(EnglishTutor.Identity.Application.IIdentityApplicationMarker).Assembly;

        var offending = new List<string>();
        foreach (var ns in new[] { AuditDomainNamespace, AuditApplicationNamespace,
                                    AuditInfrastructureNamespace, AuditContractsNamespace })
        {
            var result = Types.InAssembly(identityApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn(ns)
                .GetResult();
            if (!result.IsSuccessful)
            {
                offending.AddRange(result.FailingTypeNames ?? Array.Empty<string>());
            }
        }

        offending.Should().BeEmpty(
            "Identity.Application must NOT reference any Audit assembly. " +
            "Identity publishes security events; it does not depend on Audit. " +
            "Offending types: " + string.Join(", ", offending));
    }

    // ---- Identity.Infrastructure may reference Audit.Contracts only ----

    [Fact]
    public void Identity_Infrastructure_Should_Only_Reference_Audit_Contracts_Not_Audit_Application_Or_Domain()
    {
        var identityInfraAssembly =
            typeof(EnglishTutor.Identity.Infrastructure.IdentityInfrastructureServiceCollectionExtensions).Assembly;

        // Forbid: Audit.Domain, Audit.Application, Audit.Infrastructure.
        var forbiddenNamespaces = new[]
        {
            AuditDomainNamespace,
            AuditApplicationNamespace,
            AuditInfrastructureNamespace,
        };

        var offending = new List<string>();
        foreach (var ns in forbiddenNamespaces)
        {
            var result = Types.InAssembly(identityInfraAssembly)
                .ShouldNot()
                .HaveDependencyOn(ns)
                .GetResult();
            if (!result.IsSuccessful)
            {
                offending.AddRange(result.FailingTypeNames ?? Array.Empty<string>());
            }
        }

        offending.Should().BeEmpty(
            "Identity.Infrastructure may reference Audit.Contracts only. " +
            "Identity must not depend on Audit.Domain, Audit.Application, " +
            "or Audit.Infrastructure. Found offending types: " +
            string.Join(", ", offending));
    }

    // ---- Audit Domain aggregate-first ----

    [Fact]
    public void Audit_Domain_Types_Should_Live_Under_Aggregates_SecurityEvents()
    {
        // Every public / internal type declared in the Audit.Domain
        // assembly must live under the namespace
        // EnglishTutor.Audit.Domain.Aggregates.SecurityEvents (or one
        // of its sub-namespaces: .Errors, .Repositories, .ValueObjects,
        // .Events, .Rules, .Shared — none of which Audit currently
        // uses, but the rule accommodates them).
        //
        // No Audit-domain type may live at the Domain project root
        // (EnglishTutor.Audit.Domain) or in any non-Aggregates
        // top-level folder.
        var assembly = typeof(EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent).Assembly;

        var allowedRootNamespace = $"{AuditDomainNamespace}.Aggregates.SecurityEvents";

        var offenders = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceStartingWith(AuditDomainNamespace)
            .And()
            .DoNotResideInNamespaceStartingWith(allowedRootNamespace)
            .GetTypes()
            .Where(t => !IsCompilerGeneratedOrFramework(t))
            .Select(t => t.FullName ?? t.Name)
            .ToList();

        offenders.Should().BeEmpty(
            "All Audit domain types must live under " + allowedRootNamespace + ". " +
            "No Audit domain type may live at the Audit.Domain root outside Aggregates/. " +
            "Found offenders: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Audit_Domain_Should_Have_Exactly_One_Aggregate_Folder()
    {
        // Sanity check: there is exactly one aggregate folder under
        // src/Modules/Audit/EnglishTutor.Audit.Domain/Aggregates/.
        // Currently that is SecurityEvents. If a second aggregate is
        // added later this test will fail and prompt the author to
        // confirm the new aggregate is intentional.
        var assembly = typeof(EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent).Assembly;

        var aggregateNamespaces = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceStartingWith($"{AuditDomainNamespace}.Aggregates.")
            .GetTypes()
            .Where(t => !IsCompilerGeneratedOrFramework(t) && t.FullName != null)
            .Select(t => t.FullName!)
            .ToList();

        aggregateNamespaces.Should().NotBeEmpty(
            "Audit.Domain must contain at least one aggregate.");

        var distinctTopLevelAggregates = aggregateNamespaces
            .Select(fullName =>
            {
                var afterAggregates = fullName.Substring($"{AuditDomainNamespace}.Aggregates.".Length);
                var slash = afterAggregates.IndexOf('.');
                return slash >= 0 ? afterAggregates.Substring(0, slash) : afterAggregates;
            })
            .Distinct()
            .ToList();

        distinctTopLevelAggregates.Should().ContainSingle(
            "Audit.Domain must have exactly one aggregate folder. " +
            "Found: " + string.Join(", ", distinctTopLevelAggregates));
    }

    // Filter out compiler-generated types (e.g. <Module>, lambda
    // captures) and any framework-injected types from the architectural
    // assertion.
    private static bool IsCompilerGeneratedOrFramework(Type t)
    {
        if (t.FullName == null) return true;
        // Compiler-generated types: nested inside <Module>, generic
        // state machines, etc. — all start with '<'.
        if (t.Name.StartsWith("<", StringComparison.Ordinal)) return true;
        // Some compiler-generated nested types live under the parent
        // type but are tagged with [CompilerGenerated]. Reflect on the
        // attribute safely.
        var hasCompilerGenerated = t.GetCustomAttributes(false)
            .Any(a => a.GetType().FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        return hasCompilerGenerated;
    }
}
