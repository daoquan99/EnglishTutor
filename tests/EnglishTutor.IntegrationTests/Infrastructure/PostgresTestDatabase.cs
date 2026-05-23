using Npgsql;

namespace EnglishTutor.IntegrationTests.Infrastructure;

public sealed class PostgresTestDatabase : IAsyncDisposable
{
    private const string MaintenanceConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";
    private static readonly SemaphoreSlim CleanupLock = new(1, 1);

    public string DatabaseName { get; } = $"english_tutor_test_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Host=localhost;Port=5432;Database={DatabaseName};Username=postgres;Password=postgres";

    public bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(MaintenanceConnectionString);
            await connection.OpenAsync();
            await CleanupOldDatabasesAsync(connection);
            await using var command = new NpgsqlCommand($"""CREATE DATABASE "{DatabaseName}";""", connection);
            await command.ExecuteNonQueryAsync();
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        try
        {
            await using var connection = new NpgsqlConnection(MaintenanceConnectionString);
            await connection.OpenAsync();
            await DropDatabaseAsync(connection, DatabaseName);
        }
        catch
        {
            // Best-effort cleanup. The next test run removes stale english_tutor_test_* databases first.
        }
    }

    private static async Task CleanupOldDatabasesAsync(NpgsqlConnection connection)
    {
        await CleanupLock.WaitAsync();
        try
        {
            var databaseNames = new List<string>();
            await using (var command = new NpgsqlCommand(
                "SELECT datname FROM pg_database WHERE datistemplate = false AND datname LIKE 'english_tutor_test_%';",
                connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    databaseNames.Add(reader.GetString(0));
                }
            }

            foreach (var databaseName in databaseNames)
            {
                await DropDatabaseAsync(connection, databaseName);
            }
        }
        finally
        {
            CleanupLock.Release();
        }
    }

    private static async Task DropDatabaseAsync(NpgsqlConnection connection, string databaseName)
    {
        if (!databaseName.StartsWith("english_tutor_test_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Refusing to drop unexpected database '{databaseName}'.");
        }

        await using var terminateCommand = new NpgsqlCommand(
            """
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = @databaseName AND pid <> pg_backend_pid();
            """,
            connection);
        terminateCommand.Parameters.AddWithValue("databaseName", databaseName);
        await terminateCommand.ExecuteNonQueryAsync();

        await using var dropCommand = new NpgsqlCommand($"""DROP DATABASE IF EXISTS "{databaseName}";""", connection);
        await dropCommand.ExecuteNonQueryAsync();
    }
}
