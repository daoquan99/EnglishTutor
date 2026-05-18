using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public sealed class ProjectStructureTests
{
    private static readonly string[] Modules =
    [
        "Auth",
        "Users",
        "StudyPlans",
        "LearningContent",
        "Vocabulary",
        "Exercises",
        "Speaking",
        "AI",
        "Mistakes",
        "Assessments",
        "Progress",
        "Notifications",
        "AdminReports"
    ];

    [Fact]
    public void Modules_Should_Have_Expected_Project_Structure()
    {
        var modulesRoot = Path.Combine(GetRepositoryRoot(), "src", "Modules");

        foreach (var module in Modules)
        {
            var moduleRoot = Path.Combine(modulesRoot, module);

            Directory.Exists(moduleRoot).Should().BeTrue($"module {module} must exist");

            foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Presentation", "Contracts" })
            {
                var projectPath = Path.Combine(
                    moduleRoot,
                    $"EnglishTutor.Modules.{module}.{layer}",
                    $"EnglishTutor.Modules.{module}.{layer}.csproj");

                File.Exists(projectPath).Should().BeTrue($"{module}.{layer} project must exist");
            }
        }
    }

    [Fact]
    public void Domain_Projects_Should_Not_Reference_Application_Infrastructure_Or_Presentation()
    {
        foreach (var project in GetProjectFiles("src/Modules", "*.Domain.csproj"))
        {
            var references = GetProjectReferences(project);

            references.Should().NotContain(reference =>
                reference.Contains(".Application", StringComparison.Ordinal) ||
                reference.Contains(".Infrastructure", StringComparison.Ordinal) ||
                reference.Contains(".Presentation", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Application_Projects_Should_Not_Reference_Infrastructure_Or_Presentation()
    {
        foreach (var project in GetProjectFiles("src/Modules", "*.Application.csproj"))
        {
            var references = GetProjectReferences(project);

            references.Should().NotContain(reference =>
                reference.Contains(".Infrastructure", StringComparison.Ordinal) ||
                reference.Contains(".Presentation", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Modules_Should_Not_Reference_Other_Module_NonContract_Projects()
    {
        foreach (var project in GetProjectFiles("src/Modules", "*.csproj"))
        {
            var currentModule = GetModuleName(project);
            var references = GetProjectReferences(project)
                .Where(reference => reference.Contains("EnglishTutor.Modules.", StringComparison.Ordinal))
                .ToList();

            foreach (var reference in references)
            {
                var referencedModule = GetModuleName(reference);

                if (referencedModule == currentModule)
                {
                    continue;
                }

                Path.GetFileNameWithoutExtension(reference)
                    .Should()
                    .EndWith(".Contracts", "modules may only reference another module's Contracts project");
            }
        }
    }

    [Fact]
    public void Contracts_Projects_Should_Not_Reference_Module_Domain_Application_Infrastructure_Or_Presentation()
    {
        foreach (var project in GetProjectFiles("src/Modules", "*.Contracts.csproj"))
        {
            var references = GetProjectReferences(project);

            references.Should().NotContain(reference =>
                reference.Contains(".Domain", StringComparison.Ordinal) ||
                reference.Contains(".Application", StringComparison.Ordinal) ||
                reference.Contains(".Infrastructure", StringComparison.Ordinal) ||
                reference.Contains(".Presentation", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Module_Domain_Source_Should_Not_Use_Application_Layer_Namespaces()
    {
        var domainFiles = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src", "Modules"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => path.Contains(".Domain", StringComparison.Ordinal))
            .ToList();

        foreach (var file in domainFiles)
        {
            var source = File.ReadAllText(file);

            source.Should().NotContain("EnglishTutor.BuildingBlocks.Application");
            source.Should().NotContain(".Application;");
        }
    }

    [Fact]
    public void Module_Domain_Should_Not_Contain_Error_Catalogs()
    {
        var domainErrorCatalogs = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src", "Modules"),
            "*Errors.cs",
            SearchOption.AllDirectories)
            .Where(path =>
                path.Contains(".Domain", StringComparison.Ordinal) &&
                path.Contains($"{Path.DirectorySeparatorChar}Errors{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        domainErrorCatalogs.Should().BeEmpty("domain invariant failures should use Domain exceptions or business rules; Application owns Error catalogs");
    }

    [Fact]
    public void Project_Files_Should_Use_Central_Package_And_Build_Management()
    {
        foreach (var project in GetProjectFiles("src", "*.csproj").Concat(GetProjectFiles("tests", "*.csproj")))
        {
            var source = File.ReadAllText(project);

            source.Should().NotContain("Version=\"", "package versions belong in Directory.Packages.props");
            source.Should().NotContain("<TargetFramework>", "target framework belongs in Directory.Build.props");
            source.Should().NotContain("<Nullable>", "nullable setting belongs in Directory.Build.props");
            source.Should().NotContain("<ImplicitUsings>", "implicit usings belong in Directory.Build.props");
            source.Should().NotContain("<LangVersion>", "language version belongs in Directory.Build.props");
        }
    }

    [Fact]
    public void Contracts_Source_Should_Not_Expose_IQueryable_Or_Domain_Types()
    {
        var contractFiles = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src", "Modules"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => path.Contains(".Contracts", StringComparison.Ordinal))
            .ToList();

        foreach (var file in contractFiles)
        {
            var source = File.ReadAllText(file);

            source.Should().NotContain("IQueryable<", "Contracts must expose DTO/read-model interfaces only");
            source.Should().NotContain(".Domain", "Contracts must not expose module Domain types");
        }
    }

    [Fact]
    public void Ai_Client_Classes_Should_Exist_Only_In_Ai_Infrastructure()
    {
        var sourceFiles = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        foreach (var file in sourceFiles)
        {
            var source = File.ReadAllText(file);
            if (!source.Contains("class GeminiClient", StringComparison.Ordinal) &&
                !source.Contains("class GemmaClient", StringComparison.Ordinal))
            {
                continue;
            }

            file.Should().Contain(
                Path.Combine("Modules", "AI", "EnglishTutor.Modules.AI.Infrastructure"),
                "provider clients must stay inside AI.Infrastructure");
        }
    }

    [Fact]
    public void Non_Ai_Modules_Should_Not_Reference_Provider_Client_Names()
    {
        var nonAiSourceFiles = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src", "Modules"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}AI{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        foreach (var file in nonAiSourceFiles)
        {
            var source = File.ReadAllText(file);

            source.Should().NotContain("GeminiClient");
            source.Should().NotContain("GemmaClient");
            source.Should().NotContain("Google.AI");
        }
    }

    [Fact]
    public void Module_Source_Should_Not_Reference_Other_Module_Infrastructure_Namespaces()
    {
        var moduleFiles = Directory.GetFiles(
            Path.Combine(GetRepositoryRoot(), "src", "Modules"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        foreach (var file in moduleFiles)
        {
            var currentModule = GetModuleName(file);
            var source = File.ReadAllText(file);

            foreach (var module in Modules.Where(module => module != currentModule))
            {
                source.Should().NotContain(
                    $"EnglishTutor.Modules.{module}.Infrastructure",
                    "modules must not reference another module's Infrastructure namespace");
            }
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "EnglishTutor.slnx")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the repository root should contain EnglishTutor.slnx");
        return directory!.FullName;
    }

    private static IEnumerable<string> GetProjectFiles(string relativeRoot, string searchPattern)
    {
        var root = Path.Combine(GetRepositoryRoot(), relativeRoot);
        return Directory.GetFiles(root, searchPattern, SearchOption.AllDirectories);
    }

    private static List<string> GetProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var document = XDocument.Load(projectPath);

        return document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFullPath(Path.Combine(projectDirectory, value!)))
            .ToList();
    }

    private static string GetModuleName(string path)
    {
        var normalizedPath = path.Replace('\\', '/');
        var marker = "/Modules/";
        var markerIndex = normalizedPath.IndexOf(marker, StringComparison.Ordinal);

        markerIndex.Should().BeGreaterThanOrEqualTo(0, $"path should be inside src/Modules: {path}");

        var start = markerIndex + marker.Length;
        var end = normalizedPath.IndexOf('/', start);

        return end < 0
            ? normalizedPath[start..]
            : normalizedPath[start..end];
    }
}
