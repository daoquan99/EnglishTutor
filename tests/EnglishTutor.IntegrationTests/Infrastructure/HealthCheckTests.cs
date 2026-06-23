using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace EnglishTutor.IntegrationTests.Infrastructure;

// Integration tests for the readiness health check endpoints exposed by
// EnglishTutor.Api.
//
// Test 1 verifies the host configures health checks (does not require Docker).
// Test 2 (skipped when Docker is unavailable) verifies the actual checks
// report Healthy against the local docker-compose stack.
//
// Uses a subclass of IntegrationTestFactory so the baseline Jwt:SigningKey
// is always configured; the test-specific factory only adds
// infrastructure overrides.
//
// All integration tests share the "EnglishTutorIntegrationTests" xUnit
// collection so they run sequentially. IntegrationTestFactory uses
// process-wide environment variables (ConnectionStrings__Default,
// ConnectionStrings__Audit, Database__ApplyAuditMigrationsOnStartup,
// SeedData__Owner__Password) set in its constructor. Parallel test
// classes would race on these env vars and the factories would
// collide on the wrong database. See IntegrationTestFactory for the
// rationale and the env-var set.
[Collection("EnglishTutorIntegrationTests")]
public class HealthCheckTests
{
    private const string UnreachableConnectionString =
        "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1";

    private const string UnreachableRedisConfiguration = "127.0.0.1:1,abortConnect=false,connectTimeout=500";

    // Custom factory that extends IntegrationTestFactory with test-specific
    // infrastructure overrides (intentionally unreachable).
    private sealed class UnreachableInfraTestFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = UnreachableConnectionString,
                    ["RabbitMq:HostName"] = "127.0.0.1",
                    ["RabbitMq:Port"] = "1",
                    ["RabbitMq:UserName"] = "none",
                    ["RabbitMq:Password"] = "none",
                    ["RabbitMq:VirtualHost"] = "/",
                    ["Redis:Configuration"] = UnreachableRedisConfiguration,
                });
            });
        }
    }

    // Custom factory for Options_Should_Be_Registered_With_Defaults_From_Configuration
    // which only overrides infra values (no Jwt override needed because
    // IntegrationTestFactory already provides them).
    private sealed class LocalInfraTestFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = UnreachableConnectionString,
                    ["RabbitMq:HostName"] = "test-host",
                    ["RabbitMq:Port"] = "5672",
                    ["RabbitMq:UserName"] = "test-user",
                    ["RabbitMq:Password"] = "test-pass",
                    ["RabbitMq:VirtualHost"] = "/",
                    ["Redis:Configuration"] = "localhost:6379",
                });
            });
        }
    }

    [Fact]
    public async Task HealthChecks_Should_Be_Registered_With_Infrastructure_Tags()
    {
        // Arrange: build the API host with intentionally unreachable endpoints.
        await using var factory = new UnreachableInfraTestFactory();
        using var client = factory.CreateClient();

        // Act: hit /health. With unreachable deps this returns 503 but the body
        // still includes every registered check by name.
        var response = await client.GetAsync("/health");

        // Assert: body mentions all three infrastructure check names regardless of status.
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("postgres");
        body.Should().Contain("rabbitmq");
        body.Should().Contain("redis");
    }

    [Fact]
    public async Task HealthReady_Should_Report_Unhealthy_When_Dependencies_Unreachable()
    {
        // Arrange
        await using var factory = new UnreachableInfraTestFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert: the endpoint responds (likely 503 Service Unavailable) but
        // does not crash. Body explains which dependency failed.
        response.IsSuccessStatusCode.Should().BeFalse(
            "with unreachable dependencies, /health/ready must NOT report success");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task HealthLive_Should_Always_Succeed()
    {
        // Arrange
        await using var factory = new UnreachableInfraTestFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert: liveness does NOT depend on infrastructure, must always be 200.
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Options_Should_Be_Registered_With_Defaults_From_Configuration()
    {
        // Arrange
        await using var factory = new LocalInfraTestFactory();

        // Act
        using var scope = factory.Services.CreateScope();
        var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var rabbitOptions = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var redisOptions = scope.ServiceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;

        // Assert
        dbOptions.Default.Should().Be(UnreachableConnectionString);
        rabbitOptions.HostName.Should().Be("test-host");
        rabbitOptions.Port.Should().Be(5672);
        rabbitOptions.UserName.Should().Be("test-user");
        rabbitOptions.Password.Should().Be("test-pass");
        redisOptions.Configuration.Should().Be("localhost:6379");
    }
}
