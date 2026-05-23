extern alias WorkerHost;

using EnglishTutor.IntegrationTests.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WorkerHost::EnglishTutor.Worker.Outbox;
using Xunit;

namespace EnglishTutor.IntegrationTests;

public sealed class DatabaseMigrationIntegrationTests(DatabaseEnglishTutorApiFactory factory)
    : IClassFixture<DatabaseEnglishTutorApiFactory>
{
    [Fact]
    public async Task ApiAndMessagingMigrations_ShouldCreateModuleSchemasAndCoreTables()
    {
        if (!factory.Database.IsAvailable)
        {
            return;
        }

        using var client = factory.CreateClient();
        var healthResponse = await client.GetAsync("/health");
        healthResponse.EnsureSuccessStatusCode();

        await using var workerProvider = IntegrationWorkerHost.CreateServiceProvider(factory.Database.ConnectionString);
        await using (var workerScope = workerProvider.CreateAsyncScope())
        {
            await workerScope.ServiceProvider.GetRequiredService<MessagingDbContext>().Database.MigrateAsync();
        }

        await using var apiScope = factory.Services.CreateAsyncScope();
        var authDbContext = apiScope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var usersDbContext = apiScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        (await authDbContext.Database.CanConnectAsync()).Should().BeTrue();
        (await usersDbContext.Database.CanConnectAsync()).Should().BeTrue();

        await using var connection = new NpgsqlConnection(factory.Database.ConnectionString);
        await connection.OpenAsync();

        var tables = await ReadTableNamesAsync(connection);
        tables.Should().Contain(["auth.Users", "auth.OutboxMessages", "users.UserProfiles", "users.InboxMessages", "messaging.DeadLetterMessages"]);
    }

    private static async Task<IReadOnlyList<string>> ReadTableNamesAsync(NpgsqlConnection connection)
    {
        var tables = new List<string>();
        await using var command = new NpgsqlCommand(
            """
            SELECT schemaname || '.' || tablename
            FROM pg_tables
            WHERE schemaname IN ('auth', 'users', 'messaging')
            ORDER BY schemaname, tablename;
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
