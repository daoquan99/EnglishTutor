using FluentAssertions;
using System.Text.RegularExpressions;

namespace EnglishTutor.ArchitectureTests;

public sealed class ApiResponseGovernanceTests
{
    private static readonly Regex RawAspNetResult = new(@"\bResults\.", RegexOptions.Compiled);

    [Fact]
    public void Module_Endpoints_Should_Use_Canonical_ApiResults()
    {
        var solutionRoot = ResolveSolutionRoot();
        var modulesRoot = Path.Combine(solutionRoot, "src", "03.Modules");
        var violations = Directory
            .EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains(".Presentation/Endpoints/", StringComparison.Ordinal))
            .Where(path => RawAspNetResult.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(solutionRoot, path).Replace('\\', '/'))
            .OrderBy(path => path)
            .ToArray();

        violations.Should().BeEmpty(
            "module endpoints must return the canonical ApiResponse<T> through ApiResults instead of raw ASP.NET Results.* responses");
    }

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

        throw new DirectoryNotFoundException("Could not locate the EnglishTutor solution root.");
    }
}
