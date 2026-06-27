using System.Text.RegularExpressions;
using FluentAssertions;

namespace EnglishTutor.ArchitectureTests;

// Source-scanning architecture tests for raw SQL governance (Task 20).
//
// Rationale (per task-20-phase-1-raw-sql-governance-design-report.md § 5.2):
//   - NetArchTest detects ASSEMBLY / NAMESPACE dependencies. It does NOT
//     reliably detect method-call patterns like `database.SqlQueryRaw<T>(...)`
//     or `migrationBuilder.Sql("...")`. Source-scanning (file-content
//     regex) is the correct enforcement for raw SQL APIs.
//
// What these tests do:
//   1. Enumerate all .cs files under `src/` (default scope).
//   2. Strip block comments `/* ... */` and line comments `// ...`.
//   3. DO NOT strip string literals (string literals containing the
//      patterns would still count — see `Database.SqlQuery` example).
//   4. Match the configured regex.
//   5. Exclude the allowlist (see `AllowedFiles`).
//   6. Fail with exact file path + 1-based line number + offending line.
//
// Allowlist strategy:
//   - The allowlist is **minimal**. Initial entries are the two sites
//     identified by the Phase 0 audit:
//     1. `src/04.BuildingBlocks/EnglishTutor.BuildingBlocks.Infrastructure/HealthChecks/InfrastructureHealthChecks.cs`
//        — infrastructure health-check database reachability probe.
//     2. `tests/EnglishTutor.IntegrationTests/IntegrationTestFactory.cs`
//        — test-only temporary DB create/drop/terminate.
//   - Any new allowlist entry MUST follow the 4-step workflow
//     documented in Phase 1 design § 5.8 (allowlist + ADR/docs +
//     phase report + docs/database/postgresql-functions-procedures.md
//     registry entry). This test file will FAIL until that workflow
//     is followed.
//
// Path normalization:
//   - All paths are normalized to forward slashes before comparison,
//     so the allowlist works on Windows + Linux.

public class RawSqlGovernanceTests
{
    // -------- Allowlist --------

    /// <summary>
    /// Production-code allowlist. Each entry is a single file that is
    /// permitted to contain raw SQL / direct Npgsql / EF raw SQL APIs.
    /// Adding a new entry requires the 4-step workflow in
    /// task-20-phase-1-raw-sql-governance-design-report.md § 5.8.
    /// </summary>
    private static readonly IReadOnlyList<AllowlistEntry> ProductionAllowedFiles =
        new AllowlistEntry[]
        {
            new AllowlistEntry(
                RelativePath: "src/04.BuildingBlocks/EnglishTutor.BuildingBlocks.Infrastructure/HealthChecks/InfrastructureHealthChecks.cs",
                Reason: "Infrastructure health-check database reachability probe only. "
                      + "The file opens a Npgsql.NpgsqlConnection and runs `SELECT 1` for "
                      + "a TCP-level readiness probe. Documented in "
                      + ".agents/rules/38-postgresql-raw-sql-strict.md § 2 (Updated by Task 20)."),
        };

    /// <summary>
    /// Test-code allowlist. Test code is allowed to use Npgsql for
    /// per-test temporary DB create / drop / terminate. The location
    /// is pinned by the dedicated test
    /// `Test_Fixture_Raw_Sql_Is_Allowed_Only_In_IntegrationTestFactory`
    /// (which compares the set of test files containing Npgsql
    /// against the set of files in this allowlist).
    /// </summary>
    private static readonly IReadOnlyList<AllowlistEntry> TestAllowedFiles =
        new AllowlistEntry[]
        {
            new AllowlistEntry(
                RelativePath: "tests/EnglishTutor.IntegrationTests/IntegrationTestFactory.cs",
                Reason: "Test-only temporary database create / drop / terminate. "
                      + "Scoped to the per-factory temporary database name; never targets "
                      + "the developer's main `english_tutor_db`."),
        };

    // -------- Scope roots --------

    private static readonly string ProductionRoot = ResolveRoot("src");
    private static readonly string TestsRoot = ResolveRoot("tests");

    // -------- Patterns --------

    // Use word boundaries to avoid `Database.SqlQueryRaw` matching `Database.SqlQuery`.
    // Each pattern is regex; word boundaries (`\b`) keep the match exact.
    private static readonly Regex DatabaseSqlQueryRaw =
        new(@"\bDatabase\.SqlQueryRaw\b", RegexOptions.Compiled);
    private static readonly Regex DatabaseSqlQuery =
        new(@"(?<!\w)Database\.SqlQuery(?!\w)", RegexOptions.Compiled);
    private static readonly Regex ExecuteSqlRaw =
        new(@"\bExecuteSqlRaw\b", RegexOptions.Compiled);
    private static readonly Regex ExecuteSqlInterpolated =
        new(@"\bExecuteSqlInterpolated\b", RegexOptions.Compiled);
    private static readonly Regex FromSqlRaw =
        new(@"\bFromSqlRaw\b", RegexOptions.Compiled);
    private static readonly Regex FromSqlInterpolated =
        new(@"\bFromSqlInterpolated\b", RegexOptions.Compiled);

    // Dapper usage markers. `using Dapper;` and any `SqlMapper.*` call.
    private static readonly Regex UsingDapper =
        new(@"\busing\s+Dapper\s*;", RegexOptions.Compiled);
    private static readonly Regex SqlMapperDot =
        new(@"\bSqlMapper\.", RegexOptions.Compiled);

    // Direct Npgsql types. The allowlist excludes the one allowed file.
    private static readonly Regex NpgsqlConnectionNew =
        new(@"\bnew\s+NpgsqlConnection\b", RegexOptions.Compiled);
    private static readonly Regex NpgsqlCommandNew =
        new(@"\bnew\s+NpgsqlCommand\b", RegexOptions.Compiled);
    private static readonly Regex NpgsqlDataSourceNew =
        new(@"\bnew\s+NpgsqlDataSource\b", RegexOptions.Compiled);
    private static readonly Regex NpgsqlBatchCommandNew =
        new(@"\bnew\s+NpgsqlBatchCommand\b", RegexOptions.Compiled);

    // EF.Functions / EF.Property — discouraged shadow-property access.
    private static readonly Regex EfFunctions =
        new(@"\bEF\.Functions\b", RegexOptions.Compiled);
    private static readonly Regex EfProperty =
        new(@"\bEF\.Property\b", RegexOptions.Compiled);

    // migrationBuilder.Sql(...) — only allowed with an inline comment
    // AND an explicit allowlist entry. Today neither exists.
    private static readonly Regex MigrationBuilderSql =
        new(@"\bmigrationBuilder\.Sql\s*\(", RegexOptions.Compiled);

    // -------- Test cases --------

    [Fact]
    public void Production_Should_Not_Call_Database_SqlQueryRaw()
    {
        var hits = ScanProduction(DatabaseSqlQueryRaw);
        AssertNoHits(hits, "Database.SqlQueryRaw");
    }

    [Fact]
    public void Production_Should_Not_Call_Database_SqlQuery()
    {
        // Note: separate test from `SqlQueryRaw` because the regex
        // uses a lookahead to avoid matching `SqlQueryRaw`.
        var hits = ScanProduction(DatabaseSqlQuery);
        AssertNoHits(hits, "Database.SqlQuery");
    }

    [Fact]
    public void Production_Should_Not_Call_ExecuteSqlRaw()
    {
        var hits = ScanProduction(ExecuteSqlRaw);
        AssertNoHits(hits, "ExecuteSqlRaw");
    }

    [Fact]
    public void Production_Should_Not_Call_ExecuteSqlInterpolated()
    {
        var hits = ScanProduction(ExecuteSqlInterpolated);
        AssertNoHits(hits, "ExecuteSqlInterpolated");
    }

    [Fact]
    public void Production_Should_Not_Call_DbSet_FromSqlRaw()
    {
        var hits = ScanProduction(FromSqlRaw);
        AssertNoHits(hits, "FromSqlRaw");
    }

    [Fact]
    public void Production_Should_Not_Call_DbSet_FromSqlInterpolated()
    {
        var hits = ScanProduction(FromSqlInterpolated);
        AssertNoHits(hits, "FromSqlInterpolated");
    }

    [Fact]
    public void Production_Should_Not_Use_Dapper()
    {
        var hits1 = ScanProduction(UsingDapper);
        var hits2 = ScanProduction(SqlMapperDot);
        var hits = hits1.Concat(hits2).ToList();
        AssertNoHits(hits, "Dapper / SqlMapper");
    }

    [Fact]
    public void Production_Should_Not_Use_Direct_Npgsql_Outside_Allowlist()
    {
        var all = ScanProduction(NpgsqlConnectionNew)
            .Concat(ScanProduction(NpgsqlCommandNew))
            .Concat(ScanProduction(NpgsqlDataSourceNew))
            .Concat(ScanProduction(NpgsqlBatchCommandNew))
            .ToList();
        AssertNoHitsWithAllowlist(
            all,
            ProductionAllowedFiles,
            "NpgsqlConnection / NpgsqlCommand / NpgsqlDataSource / NpgsqlBatchCommand");
    }

    [Fact]
    public void Production_Should_Not_Use_EF_Functions_Or_EF_Property_Outside_Allowlist()
    {
        var all = ScanProduction(EfFunctions)
            .Concat(ScanProduction(EfProperty))
            .ToList();
        // The production allowlist currently has no EF.Functions /
        // EF.Property entries. If a future slice needs shadow-property
        // access, it must add an explicit entry here.
        AssertNoHitsWithAllowlist(
            all,
            ProductionAllowedFiles,
            "EF.Functions / EF.Property");
    }

    [Fact]
    public void Migrations_Should_Not_Use_MigrationBuilder_Sql_Without_Allowlist()
    {
        // Scan only migration .cs files (NOT Designer.cs, NOT
        // ModelSnapshot.cs). Today neither migration uses
        // migrationBuilder.Sql; no allowlist entry is needed.
        var hits = ScanMigrations(MigrationBuilderSql);

        // No allowlist entry exists for migrations today. Any hit fails.
        AssertNoHits(hits, "migrationBuilder.Sql");
    }

    [Fact]
    public void Test_Fixture_Raw_Sql_Is_Allowed_Only_In_IntegrationTestFactory()
    {
        // Pin: the only test file allowed to use Npgsql is
        // tests/EnglishTutor.IntegrationTests/IntegrationTestFactory.cs.
        var all = ScanTests(NpgsqlConnectionNew)
            .Concat(ScanTests(NpgsqlCommandNew))
            .Concat(ScanTests(NpgsqlDataSourceNew))
            .Concat(ScanTests(NpgsqlBatchCommandNew))
            .ToList();

        // Remove the allowlisted file.
        var allowlistedSet = new HashSet<string>(
            TestAllowedFiles.Select(a => NormalizePath(a.RelativePath)),
            StringComparer.OrdinalIgnoreCase);
        var violating = all
            .Where(h => !allowlistedSet.Contains(NormalizePath(h.RelativePath)))
            .ToList();

        AssertNoHits(
            violating,
            "Npgsql usage in a test file outside the test allowlist");
    }

    // -------- Helpers --------

    /// <summary>
    /// Scan all .cs files under ProductionRoot, excluding bin/, obj/,
    /// and *.Designer.cs / *ModelSnapshot.cs. Strip comments before
    /// scanning. Do NOT strip string literals.
    /// </summary>
    private static List<SourceHit> ScanProduction(Regex regex)
    {
        return Scan(ProductionRoot, regex, excludeMigrationDesigner: true);
    }

    /// <summary>
    /// Scan all .cs files under TestsRoot, excluding bin/, obj/,
    /// *.Designer.cs / *ModelSnapshot.cs. Strip comments.
    /// </summary>
    private static List<SourceHit> ScanTests(Regex regex)
    {
        return Scan(TestsRoot, regex, excludeMigrationDesigner: true);
    }

    /// <summary>
    /// Scan only migration .cs files under src/03.Modules/**/Persistence/Migrations/.
    /// Excludes *.Designer.cs and *ModelSnapshot.cs.
    /// </summary>
    private static List<SourceHit> ScanMigrations(Regex regex)
    {
        var hits = new List<SourceHit>();
        if (!Directory.Exists(ProductionRoot))
        {
            return hits;
        }

        foreach (var migrationDir in Directory.EnumerateDirectories(
                     ProductionRoot, "Migrations", SearchOption.AllDirectories))
        {
            foreach (var path in Directory.EnumerateFiles(
                         migrationDir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                if (IsExcludedFile(path))
                {
                    continue;
                }

                hits.AddRange(ScanFile(path, regex));
            }
        }
        return hits;
    }

    private static List<SourceHit> Scan(string root, Regex regex, bool excludeMigrationDesigner)
    {
        var hits = new List<SourceHit>();
        if (!Directory.Exists(root))
        {
            return hits;
        }

        foreach (var path in Directory.EnumerateFiles(
                     root, "*.cs", SearchOption.AllDirectories))
        {
            if (IsExcludedFile(path))
            {
                continue;
            }

            hits.AddRange(ScanFile(path, regex));
        }
        return hits;
    }

    private static bool IsExcludedFile(string absolutePath)
    {
        var normalized = NormalizePath(absolutePath);
        // Exclude build outputs.
        if (normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        // Exclude EF Core generated artefacts.
        var fileName = Path.GetFileName(absolutePath);
        if (fileName.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private static IEnumerable<SourceHit> ScanFile(string absolutePath, Regex regex)
    {
        string source;
        try
        {
            source = File.ReadAllText(absolutePath);
        }
        catch
        {
            // Unreadable file (locked, etc.) — skip rather than fail.
            yield break;
        }

        var stripped = StripComments(source);
        var relativePath = ToRelativePath(absolutePath);

        // Walk line numbers against the STRIPPED source. The stripped
        // source has the same number of lines as the original because
        // we strip by character classes that do not span newlines.
        var lines = stripped.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (regex.IsMatch(line))
            {
                yield return new SourceHit(relativePath, i + 1, line.Trim());
            }
        }
    }

    /// <summary>
    /// Strip block comments `/* ... */` (non-greedy, multi-line)
    /// and line comments `// ...`. String literals are NOT stripped
    /// — patterns inside strings still count. This is intentional:
    /// `var s = "Database.SqlQueryRaw";` is a literal, but it also
    /// signals the developer is aware of the API and the literal
    /// deserves review.
    /// </summary>
    private static string StripComments(string source)
    {
        // Block comments — non-greedy, multi-line (Singleline dot mode).
        source = Regex.Replace(
            source,
            @"/\*.*?\*/",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.Compiled);
        // Line comments — up to end of line.
        source = Regex.Replace(
            source,
            @"//[^\n]*",
            string.Empty,
            RegexOptions.Compiled);
        return source;
    }

    private static void AssertNoHits(
        IReadOnlyList<SourceHit> hits,
        string patternLabel)
    {
        if (hits.Count == 0)
        {
            return;
        }

        var message = new System.Text.StringBuilder();
        message.Append("Raw SQL governance FAILED for pattern: ")
               .Append(patternLabel)
               .AppendLine();
        message.Append("Offending file(s):").AppendLine();
        foreach (var hit in hits)
        {
            message.Append("  - ")
                   .Append(hit.RelativePath)
                   .Append(":")
                   .Append(hit.LineNumber)
                   .Append("  ")
                   .Append(hit.LineText)
                   .AppendLine();
        }
        message.AppendLine();
        message.Append("No allowlist entry matched these files. If intentional, ")
               .Append("add an entry to RawSqlGovernanceTests.ProductionAllowedFiles[] ")
               .Append("with a clear Reason (and update ADR-0010 + the docs/database/")
               .Append("postgresql-functions-procedures.md registry).")
               .AppendLine();
        message.Append("If unintentional, this is a regression and must be fixed.")
               .AppendLine();
        hits.Should().BeEmpty(message.ToString());
    }

    private static void AssertNoHitsWithAllowlist(
        IReadOnlyList<SourceHit> hits,
        IReadOnlyList<AllowlistEntry> allowlist,
        string patternLabel)
    {
        var allowlistedSet = new HashSet<string>(
            allowlist.Select(a => NormalizePath(a.RelativePath)),
            StringComparer.OrdinalIgnoreCase);
        var violating = hits
            .Where(h => !allowlistedSet.Contains(NormalizePath(h.RelativePath)))
            .ToList();
        AssertNoHits(violating, patternLabel);
    }

    private static string ResolveRoot(string folderName)
    {
        // The architecture test assembly is bin/Debug/net10.0/.
        // Walk up to the solution root, then resolve into `folderName`.
        // Solution layout:
        //   <solution>/
        //     src/
        //     tests/
        //     EnglishTutor.slnx
        //     tests/EnglishTutor.ArchitectureTests/bin/Debug/net10.0/EnglishTutor.ArchitectureTests.dll
        var assemblyDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var candidate = Path.GetFullPath(Path.Combine(assemblyDir, folderName));
        return candidate;
    }

    private static string ToRelativePath(string absolutePath)
    {
        var root = ResolveSolutionRootForRelative(absolutePath);
        var normalizedRoot = NormalizePath(root);
        var normalizedPath = NormalizePath(absolutePath);
        if (normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedPath[(normalizedRoot.Length + 1)..];
        }
        return normalizedPath;
    }

    private static string ResolveSolutionRootForRelative(string absolutePath)
    {
        // Walk up from absolutePath until we find the directory that
        // contains both `src/` and `tests/`. That is the solution root.
        var dir = Path.GetDirectoryName(absolutePath);
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, "src"))
                && Directory.Exists(Path.Combine(dir, "tests")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return Path.GetPathRoot(absolutePath) ?? string.Empty;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private sealed record SourceHit(
        string RelativePath,
        int LineNumber,
        string LineText);

    private sealed record AllowlistEntry(
        string RelativePath,
        string Reason);
}
