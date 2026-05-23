using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EnglishTutor.IntegrationTests;

public sealed class ApiHostSmokeTests : IClassFixture<EnglishTutorApiFactory>
{
    private readonly HttpClient _client;

    public ApiHostSmokeTests(EnglishTutorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        // /health/live is the liveness probe — checks the process is running, not external deps.
        // The full /health endpoint requires Postgres + Redis which the smoke test does not provide.
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SwaggerJson_ShouldBeAvailableInDevelopment()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public sealed class EnglishTutorApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:ValidateMigrations", "false");
        builder.UseSetting("Jwt:Secret", "super-secret-key-for-smoke-tests-min-32-chars");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:AutoMigrate"] = "false",
                ["Database:ValidateMigrations"] = "false",
                ["SeedData:Enabled"] = "false",
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
                ["Cors:AllowCredentials"] = "true",
                ["Jwt:Secret"] = "super-secret-key-for-smoke-tests-min-32-chars",
                ["Storage:Provider"] = "Local",
                ["Storage:LocalPath"] = Path.Combine(Path.GetTempPath(), "english-tutor-integration-tests"),
                ["Storage:BaseUrl"] = "/storage",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=english_tutor_test_smoke;Username=postgres;Password=postgres",
                ["ConnectionStrings:Redis"] = "localhost:6379"
            });
        });
    }
}
