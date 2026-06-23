using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace EnglishTutor.IntegrationTests;

// Custom WebApplicationFactory pre-configured with a valid Jwt:SigningKey
// and other test-only defaults. Use this everywhere instead of
// `new WebApplicationFactory<Program>()` so the Slice 2.7 JwtBearer
// registration does not throw IDX10703 at host startup.
//
// Why this exists. The Slice 2.7 JwtBearer registration in
// IdentityInfrastructureServiceCollectionExtensions reads
// Jwt:SigningKey at DI build time and constructs a
// SymmetricSecurityKey. A missing or empty key throws IDX10703 the
// moment the host starts. Without this factory, every test that
// instantiates a fresh WebApplicationFactory<Program> (including
// xUnit IClassFixture-based tests like CorrelationIdMiddlewareTests)
// would fail with that exception before any test code runs.
//
// What this factory does NOT do:
//   - It does not weaken the production JwtBearer registration.
//   - It does not share a signing key with any real environment.
//   - It does not apply UseEnvironment("Development") by default.
//     Individual tests may opt in via WithWebHostBuilder.
//
// Test isolation. Each factory instance creates a UNIQUE temporary
// PostgreSQL database (`english_tutor_test_<guid>`) and overrides
// `ConnectionStrings:Default` + `ConnectionStrings:Audit` to point to
// it. Program.cs runs `Database.MigrateAsync()` on startup, which
// creates the database and applies all pending migrations
// (Identity + Audit, since this factory sets
// `Database:ApplyAuditMigrationsOnStartup=true`). The Identity seeder
// then runs with the deterministic `SeedData:Owner:Password`
// configured below.
//
// On Dispose the factory drops the temporary database. Best-effort —
// teardown errors are swallowed so a failed teardown never hides a
// failed assertion.
//
// The factory does NOT touch the developer's main local DB
// (`english_tutor_db`). It does NOT use destructive Docker volume
// commands. It does NOT call `dotnet ef database drop --force`.
// Dropping the per-factory temporary database is the only DDL/DML it
// performs against the local Postgres instance, and it is targeted at
// the database name it generated itself.
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSigningKey =
        "integration-test-signing-key-32-bytes-min-please-do-not-reuse";

    public const string TestJwtIssuer = "EnglishTutor.IntegrationTests";
    public const string TestJwtAudience = "EnglishTutor.IntegrationTests";
    public const string TestSeedOwnerPassword = "integration-test-owner-password";

    private const string PostgresAdminConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=englishtutor;Password=englishtutor_dev";

    private readonly string _testDbName = $"english_tutor_test_{Guid.NewGuid():N}";

    public string TestConnectionString =>
        $"Host=localhost;Port=5432;Database={_testDbName};Username=englishtutor;Password=englishtutor_dev";

    public IntegrationTestFactory()
    {
        // ASP.NET Core's default configuration sources read environment
        // variables with `__` as the section separator
        // (`ConnectionStrings__Default` => `ConnectionStrings:Default`).
        // Env vars are layered AFTER appsettings.json by the default
        // host builder, so this override is guaranteed to win over the
        // checked-in appsettings.json. Using env vars instead of
        // ConfigureAppConfiguration ensures the connection string is
        // picked up by the DbContext registration lambdas, which
        // capture `IConfiguration` references at registration time and
        // read them lazily at resolution time.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", TestConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Audit", TestConnectionString);
        Environment.SetEnvironmentVariable("Database__ApplyAuditMigrationsOnStartup", "true");
        Environment.SetEnvironmentVariable("SeedData__Owner__Password", TestSeedOwnerPassword);

        // Pre-create the per-factory test database with TEMPLATE template0.
        // The local dev postgres image has a collation-version mismatch
        // on template1 which causes plain CREATE DATABASE to fail with
        // "template database template1 has a collation version, but no
        // actual collation version could be determined". template0 has
        // no per-collation metadata and is the standard escape hatch for
        // a fresh database that needs no template data. EF Core's
        // MigrateAsync later sees the database exists and applies
        // migrations on top.
        TryCreateTestDatabaseWithTemplate0();
    }

    private void TryCreateTestDatabaseWithTemplate0()
    {
        try
        {
            using var conn = new NpgsqlConnection(PostgresAdminConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "CREATE DATABASE \"" + _testDbName.Replace("\"", "\"\"") +
                "\" TEMPLATE template0", conn);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException)
        {
            // Best-effort: if the DB already exists from a previous
            // run that crashed mid-cleanup, MigrateAsync will still
            // apply pending migrations. If creation fails for any
            // other reason, the test will surface the original
            // NpgsqlException when it tries to use the DB.
        }
    }

    public static IDictionary<string, string?> DefaultConfiguration() =>
        new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = TestJwtSigningKey,
            ["Jwt:Issuer"] = TestJwtIssuer,
            ["Jwt:Audience"] = TestJwtAudience,
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "7",
            ["SeedData:Owner:Password"] = TestSeedOwnerPassword,
        };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Base test configuration (JWT, seed password, etc.).
            // The connection strings, audit-migration flag, and seed
            // password are also set as environment variables in the
            // constructor above so they take effect at registration
            // time. We still set the seed password here so the
            // AuthFlowTestFactory subclass can override it via its
            // own ConfigureWebHost pass.
            config.AddInMemoryCollection(DefaultConfiguration());
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DropTestDatabase();
        }
        base.Dispose(disposing);
    }

    // Best-effort drop of the per-factory temporary database. Runs only
    // against the DB name this factory generated; never targets the
    // main `english_tutor_db` database.
    private void DropTestDatabase()
    {
        try
        {
            using var conn = new NpgsqlConnection(PostgresAdminConnectionString);
            conn.Open();

            // Disconnect any active connections to the test DB so DROP
            // can succeed. We filter by datname to scope the terminate.
            using (var discCmd = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
                "WHERE datname = @dbname AND pid <> pg_backend_pid()", conn))
            {
                discCmd.Parameters.AddWithValue("dbname", _testDbName);
                discCmd.ExecuteNonQuery();
            }

            using var dropCmd = new NpgsqlCommand(
                "DROP DATABASE IF EXISTS " +
                "\"" + _testDbName.Replace("\"", "\"\"") + "\"", conn);
            dropCmd.ExecuteNonQuery();
        }
        catch
        {
            // Best-effort teardown; never let teardown errors mask test
            // failures.
        }
    }
}
