using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace EnglishTutor.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSigningKey =
        "local-dev-jwt-signing-key-32-characters-minimum";

    public const string TestAiGatewayEncryptionMasterKey =
        "local-dev-ai-gateway-master-key-32-characters-minimum";

    public const string TestJwtIssuer = "EnglishTutor.IntegrationTests";
    public const string TestJwtAudience = "EnglishTutor.IntegrationTests";
    public const string TestSeedOwnerPassword = "integration-test-owner-password";

    private const string PostgresAdminConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=englishtutor;Password=englishtutor_dev";

    private readonly string _testDbName = $"english_tutor_test_{Guid.NewGuid():N}";
    private int _databaseDropped;

    public string TestConnectionString =>
        $"Host=localhost;Port=5432;Database={_testDbName};Username=englishtutor;Password=englishtutor_dev";

    public IntegrationTestFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", TestConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Audit", TestConnectionString);
        Environment.SetEnvironmentVariable("Database__ApplyAuditMigrationsOnStartup", "true");
        Environment.SetEnvironmentVariable("SeedData__Owner__Password", TestSeedOwnerPassword);

        Environment.SetEnvironmentVariable("Messaging__InProcessAuditConsumer", "false");

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
            ["Jwt:Secret"] = TestJwtSigningKey,
            ["Jwt:Issuer"] = TestJwtIssuer,
            ["Jwt:Audience"] = TestJwtAudience,
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "7",
            ["AiGateway:EncryptionMasterKey"] = TestAiGatewayEncryptionMasterKey,
            ["SeedData:Owner:Password"] = TestSeedOwnerPassword,
            ["RabbitMq:HostName"] = "localhost",
            ["RabbitMq:Port"] = "5672",
            ["RabbitMq:UserName"] = "englishtutor",
            ["RabbitMq:Password"] = "englishtutor_dev",
            ["RabbitMq:VirtualHost"] = "/",
        };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(DefaultConfiguration());
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                d.ImplementationType != null &&
                IsRemovableMessagingHostedService(d.ImplementationType))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }
        });
    }

    private bool IsRemovableMessagingHostedService(Type implementationType)
    {
        var isDelivery = implementationType.Name.StartsWith("BusOutboxDeliveryService");
        var isCleanup = implementationType.Name.StartsWith("InboxCleanupService");
        if (!isDelivery && !isCleanup)
        {
            return false;
        }

        if (!KeepMessagingHostedServices)
        {
            return true;
        }

        // Keep Identity/Audit services; remove any others (like Learning) whose schema is not migrated on startup.
        var hasIdentityOrAudit = implementationType.GenericTypeArguments.Any(t =>
            t.Name.Contains("Identity") || t.Name.Contains("Audit"));
        return !hasIdentityOrAudit;
    }

    /// <summary>
    /// When true, the Identity/Audit MassTransit outbox delivery + inbox-cleanup
    /// hosted services are left running so the durable security-event flow
    /// (outbox → in-process delivery → Audit consumer) completes. Default false.
    /// </summary>
    protected virtual bool KeepMessagingHostedServices => false;

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            if (disposing)
            {
                DropTestDatabaseOnce();
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            DropTestDatabaseOnce();
        }
    }

    private void DropTestDatabaseOnce()
    {
        if (System.Threading.Interlocked.Exchange(ref _databaseDropped, 1) == 1)
        {
            return;
        }

        DropTestDatabase();
    }

    private void DropTestDatabase()
    {
        try
        {
            using var conn = new NpgsqlConnection(PostgresAdminConnectionString);
            conn.Open();

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
        }
    }
}
