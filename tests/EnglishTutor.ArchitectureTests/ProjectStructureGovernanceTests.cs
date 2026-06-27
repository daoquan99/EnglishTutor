using System.Xml.Linq;
using FluentAssertions;

namespace EnglishTutor.ArchitectureTests;

public sealed class ProjectStructureGovernanceTests
{
    private static readonly string SolutionRoot = ResolveSolutionRoot();
    private static readonly string SourceRoot = Path.Combine(SolutionRoot, "src");

    private static readonly IReadOnlyDictionary<string, BaselineMetadata>
        ProjectReferenceBaseline = new Dictionary<string, BaselineMetadata>
        {
            [ProjectReferenceKey(
                "src/02.Hosts/EnglishTutor.Api/EnglishTutor.Api.csproj",
                "EnglishTutor.Identity.Application")] = new(
                "API directly references Identity.Application.",
                "Remove after Identity exposes a complete module registration boundary."),
            [ProjectReferenceKey(
                "src/03.Modules/Identity/EnglishTutor.Identity.Presentation/EnglishTutor.Identity.Presentation.csproj",
                "EnglishTutor.Identity.Infrastructure")] = new(
                "Identity.Presentation directly references Identity.Infrastructure.",
                "Remove after cookie/infrastructure services are exposed through an allowed boundary.")
        };

    private static readonly IReadOnlyDictionary<string, BaselineMetadata>
        PackageReferenceBaseline = new Dictionary<string, BaselineMetadata>
        {
            [PackageReferenceKey(
                "src/03.Modules/Quota/EnglishTutor.Quota.Application/EnglishTutor.Quota.Application.csproj",
                "Microsoft.EntityFrameworkCore")] = new(
                "Quota.Application catches DbUpdateConcurrencyException.",
                "Remove after Infrastructure translates provider concurrency failures.")
        };

    private static readonly HashSet<string> AllowedDomainRootFiles = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "src/03.Modules/Identity/EnglishTutor.Identity.Domain/IIdentityDomainMarker.cs",
        "src/03.Modules/Identity/EnglishTutor.Identity.Domain/IdentityModuleNames.cs"
    };

    private static readonly IReadOnlyDictionary<string, BaselineMetadata>
        DomainPlacementBaseline = new Dictionary<string, BaselineMetadata>
        {
        };

    [Fact]
    public void Production_Project_References_Should_Follow_Canonical_Dependency_Matrix()
    {
        var actualViolations = DiscoverProjectReferenceViolations()
            .ToDictionary(violation => violation.Key, violation => violation);

        AssertMatchesBaseline(
            actualViolations,
            ProjectReferenceBaseline,
            "project-reference");
    }

    [Fact]
    public void Domain_And_Application_Projects_Should_Not_Reference_Provider_Packages()
    {
        var actualViolations = DiscoverForbiddenPackageReferences()
            .ToDictionary(violation => violation.Key, violation => violation);

        AssertMatchesBaseline(
            actualViolations,
            PackageReferenceBaseline,
            "forbidden-package");
    }

    [Fact]
    public void Implemented_Domain_Projects_Should_Use_Aggregate_First_Placement()
    {
        var actualViolations = DiscoverDomainPlacementViolations()
            .ToDictionary(violation => violation.Key, violation => violation);

        AssertMatchesBaseline(
            actualViolations,
            DomainPlacementBaseline,
            "domain-placement");
    }

    [Fact]
    public void Module_DbContexts_Should_Apply_AggregateRoot_SoftDelete_Conventions()
    {
        var violations = DiscoverDbContextsMissingAggregateRootConventions();

        violations.Should().BeEmpty(
            "module DbContexts must apply the shared AggregateRoot soft-delete query filter convention."
            + Environment.NewLine
            + FormatEntries(violations));
    }

    private static HashSet<ProjectReferenceViolation> DiscoverProjectReferenceViolations()
    {
        var violations = new HashSet<ProjectReferenceViolation>();

        foreach (var projectPath in EnumerateProductionProjects())
        {
            var source = ProjectIdentity.Parse(projectPath);
            var document = XDocument.Load(projectPath);

            foreach (var reference in document.Descendants("ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(include))
                {
                    continue;
                }

                var targetPath = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(projectPath)!, include));
                var target = ProjectIdentity.Parse(targetPath);

                if (!IsAllowedReference(source, target))
                {
                    violations.Add(new ProjectReferenceViolation(
                        ToRelativePath(projectPath),
                        target.Name,
                        $"Forbidden {source.Kind} -> {target.Kind} project reference.",
                        "Remove the reference or route through an allowed module contract/registration boundary."));
                }
            }
        }

        return violations;
    }

    private static HashSet<PackageViolation> DiscoverForbiddenPackageReferences()
    {
        var violations = new HashSet<PackageViolation>();

        foreach (var projectPath in EnumerateProductionProjects())
        {
            var project = ProjectIdentity.Parse(projectPath);
            if (project.Kind is not ProjectKind.Domain and not ProjectKind.Application)
            {
                continue;
            }

            var document = XDocument.Load(projectPath);
            foreach (var package in document.Descendants("PackageReference"))
            {
                var packageName = package.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(packageName))
                {
                    continue;
                }

                if (IsForbiddenPackage(project.Kind, packageName))
                {
                    violations.Add(new PackageViolation(
                        ToRelativePath(projectPath),
                        packageName,
                        $"{project.Kind} references a provider/framework package.",
                        "Move provider behavior to Infrastructure and depend on an application/domain abstraction."));
                }
            }
        }

        return violations;
    }

    private static HashSet<DomainPlacementViolation> DiscoverDomainPlacementViolations()
    {
        var violations = new HashSet<DomainPlacementViolation>();

        foreach (var domainProjectPath in EnumerateProductionProjects()
            .Where(path =>
            {
                var project = ProjectIdentity.Parse(path);
                return project.IsModule && project.Kind == ProjectKind.Domain;
            }))
        {
            var projectDirectory = Path.GetDirectoryName(domainProjectPath)!;
            var productionFiles = Directory.EnumerateFiles(
                    projectDirectory,
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(path => !IsBuildOutput(path))
                .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (productionFiles.Count == 0)
            {
                continue;
            }

            foreach (var file in productionFiles)
            {
                var relativeToProject = Path.GetRelativePath(projectDirectory, file)
                    .Replace('\\', '/');
                var relativeToSolution = ToRelativePath(file);

                if (relativeToProject.StartsWith("Aggregates/", StringComparison.OrdinalIgnoreCase)
                    || relativeToProject.StartsWith("Shared/", StringComparison.OrdinalIgnoreCase)
                    || AllowedDomainRootFiles.Contains(relativeToSolution))
                {
                    continue;
                }

                violations.Add(new DomainPlacementViolation(
                    relativeToSolution,
                    "Domain production file is outside Aggregates/ or Shared/.",
                    "Move it under its owning aggregate/shared folder or document a narrow root metadata exception."));
            }
        }

        return violations;
    }

    private static HashSet<DbContextConventionViolation> DiscoverDbContextsMissingAggregateRootConventions()
    {
        var violations = new HashSet<DbContextConventionViolation>();

        foreach (var file in Directory.EnumerateFiles(SourceRoot, "*DbContext.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}03.Modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !IsBuildOutput(path))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var source = File.ReadAllText(file);
            if (!source.Contains(": DbContext", StringComparison.Ordinal)
                || !source.Contains("DbSet<", StringComparison.Ordinal))
            {
                continue;
            }

            if (source.Contains("ApplyAggregateRootConventions()", StringComparison.Ordinal))
            {
                continue;
            }

            violations.Add(new DbContextConventionViolation(
                ToRelativePath(file),
                "DbContext does not apply AggregateRoot soft-delete conventions.",
                "Call modelBuilder.ApplyAggregateRootConventions() in OnModelCreating after entity mappings are registered."));
        }

        return violations;
    }

    private static bool IsAllowedReference(ProjectIdentity source, ProjectIdentity target)
    {
        if (source.IsBuildingBlocks)
        {
            return source.Kind switch
            {
                ProjectKind.Domain or ProjectKind.Contracts => false,
                ProjectKind.Application =>
                    target.IsBuildingBlocks
                    && target.Kind is ProjectKind.Domain or ProjectKind.Contracts,
                ProjectKind.Infrastructure =>
                    target.IsBuildingBlocks
                    && target.Kind is ProjectKind.Domain
                        or ProjectKind.Application
                        or ProjectKind.Contracts,
                _ => false
            };
        }

        if (source.Kind == ProjectKind.ApiHost)
        {
            return target.Kind == ProjectKind.ServiceDefaults
                   || target.IsBuildingBlocks && target.Kind == ProjectKind.Infrastructure
                   || target.IsModule
                   && target.Kind is ProjectKind.Presentation or ProjectKind.Infrastructure;
        }

        if (source.Kind == ProjectKind.WorkerHost)
        {
            return target.Kind == ProjectKind.ServiceDefaults
                   || target.IsBuildingBlocks && target.Kind == ProjectKind.Infrastructure
                   || target.IsModule && target.Kind == ProjectKind.Infrastructure;
        }

        if (!source.IsModule)
        {
            return true;
        }

        if (target.IsBuildingBlocks)
        {
            return source.Kind switch
            {
                ProjectKind.Domain => target.Kind == ProjectKind.Domain,
                ProjectKind.Application =>
                    target.Kind is ProjectKind.Application or ProjectKind.Contracts,
                ProjectKind.Infrastructure =>
                    target.Kind is ProjectKind.Domain
                        or ProjectKind.Application
                        or ProjectKind.Infrastructure
                        or ProjectKind.Contracts,
                ProjectKind.Presentation => target.Kind == ProjectKind.Application,
                ProjectKind.Contracts => target.Kind == ProjectKind.Contracts,
                _ => false
            };
        }

        if (!target.IsModule)
        {
            return false;
        }

        if (!string.Equals(source.Module, target.Module, StringComparison.Ordinal))
        {
            return target.Kind == ProjectKind.Contracts;
        }

        return source.Kind switch
        {
            ProjectKind.Domain => false,
            ProjectKind.Application =>
                target.Kind is ProjectKind.Domain or ProjectKind.Contracts,
            ProjectKind.Infrastructure =>
                target.Kind is ProjectKind.Domain
                    or ProjectKind.Application
                    or ProjectKind.Contracts,
            ProjectKind.Presentation =>
                target.Kind is ProjectKind.Application or ProjectKind.Contracts,
            ProjectKind.Contracts => false,
            _ => false
        };
    }

    private static bool IsForbiddenPackage(ProjectKind kind, string packageName)
    {
        string[] providerPrefixes =
        [
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "Dapper",
            "StackExchange.Redis",
            "MassTransit.RabbitMQ",
            "RabbitMQ.Client"
        ];

        if (providerPrefixes.Any(prefix =>
                packageName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (kind == ProjectKind.Domain)
        {
            string[] domainForbiddenPrefixes =
            [
                "Microsoft.AspNetCore",
                "MediatR",
                "MassTransit"
            ];

            return domainForbiddenPrefixes.Any(prefix =>
                packageName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private static IEnumerable<string> EnumerateProductionProjects() =>
        Directory.EnumerateFiles(SourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path));

    private static bool IsBuildOutput(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertMatchesBaseline<T>(
        IReadOnlyDictionary<string, T> actual,
        IReadOnlyDictionary<string, BaselineMetadata> baseline,
        string label)
    {
        var newViolations = actual.Keys.Except(baseline.Keys).ToList();
        var resolvedBaseline = baseline.Keys.Except(actual.Keys).ToList();

        newViolations.Should().BeEmpty(
            $"New {label} violations are forbidden.{Environment.NewLine}"
            + FormatEntries(newViolations.Select(key => actual[key]!)));

        resolvedBaseline.Should().BeEmpty(
            $"The following {label} baseline entries no longer reproduce. "
            + $"Remove them from the test baseline.{Environment.NewLine}"
            + FormatEntries(resolvedBaseline.Select(key =>
                $"{key} — {baseline[key].Reason} Removal: {baseline[key].RemovalTarget}")));
    }

    private static string FormatEntries<T>(IEnumerable<T> entries) =>
        string.Join(Environment.NewLine, entries.Select(entry => $"  - {entry}"));

    private static string ProjectReferenceKey(
        string sourceProject,
        string targetProject) =>
        $"{sourceProject} -> {targetProject}";

    private static string PackageReferenceKey(
        string sourceProject,
        string package) =>
        $"{sourceProject} :: {package}";

    private static string ResolveSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EnglishTutor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the EnglishTutor solution root.");
    }

    private static string ToRelativePath(string absolutePath) =>
        Path.GetRelativePath(SolutionRoot, absolutePath).Replace('\\', '/');

    private enum ProjectKind
    {
        Unknown,
        Domain,
        Application,
        Infrastructure,
        Presentation,
        Contracts,
        ApiHost,
        WorkerHost,
        ServiceDefaults
    }

    private sealed record ProjectIdentity(
        string Name,
        ProjectKind Kind,
        string? Module,
        bool IsModule,
        bool IsBuildingBlocks)
    {
        public static ProjectIdentity Parse(string projectPath)
        {
            var name = Path.GetFileNameWithoutExtension(projectPath);

            if (name == "EnglishTutor.Api")
            {
                return new(name, ProjectKind.ApiHost, null, false, false);
            }

            if (name == "EnglishTutor.Worker")
            {
                return new(name, ProjectKind.WorkerHost, null, false, false);
            }

            if (name == "EnglishTutor.ServiceDefaults")
            {
                return new(name, ProjectKind.ServiceDefaults, null, false, false);
            }

            const string buildingBlocksPrefix = "EnglishTutor.BuildingBlocks.";
            if (name.StartsWith(buildingBlocksPrefix, StringComparison.Ordinal))
            {
                var layer = name[buildingBlocksPrefix.Length..];
                return new(name, ParseLayer(layer), null, false, true);
            }

            var parts = name.Split('.');
            if (parts.Length == 3 && parts[0] == "EnglishTutor")
            {
                return new(
                    name,
                    ParseLayer(parts[2]),
                    parts[1],
                    true,
                    false);
            }

            return new(name, ProjectKind.Unknown, null, false, false);
        }

        private static ProjectKind ParseLayer(string value) =>
            Enum.TryParse<ProjectKind>(value, ignoreCase: false, out var kind)
                ? kind
                : ProjectKind.Unknown;
    }

    private sealed record ProjectReferenceViolation(
        string SourceProject,
        string TargetProject,
        string Reason,
        string RemovalTarget)
    {
        public string Key => ProjectReferenceKey(SourceProject, TargetProject);
    }

    private sealed record PackageViolation(
        string SourceProject,
        string Package,
        string Reason,
        string RemovalTarget)
    {
        public string Key => PackageReferenceKey(SourceProject, Package);
    }

    private sealed record DomainPlacementViolation(
        string File,
        string Reason,
        string RemovalTarget)
    {
        public string Key => File;
    }

    private sealed record DbContextConventionViolation(
        string File,
        string Reason,
        string RemovalTarget)
    {
        public string Key => File;
    }

    private sealed record BaselineMetadata(
        string Reason,
        string RemovalTarget);
}
