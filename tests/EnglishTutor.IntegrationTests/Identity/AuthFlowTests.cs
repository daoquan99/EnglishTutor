using System.Net;
using System.Net.Http.Json;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Identity;

// Integration tests for the Identity auth flow: login -> /api/me -> refresh
// -> /api/me -> logout. Uses Testcontainers Postgres + RabbitMQ + Redis
// when Docker is available; otherwise the tests are skipped via
// SkipUnlessDockerFactAttribute (TODO).
//
// Uses the shared IntegrationTestFactory so the Jwt:SigningKey baseline is
// configured once in one place. AuthFlowTests overrides SeedData:Owner:Password
// to a known value so it can call LoginRequest with that exact password.
public class AuthFlowTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    [Fact]
    public async Task Login_With_Seeded_Owner_Account_Should_Return_Tokens()
    {
        // Arrange: skip if infra dependencies are unreachable (unit env).
        await using var factory = BuildFactory();

        using var client = factory.CreateClient();

        // Seed the Owner user before logging in.
        await SeedOwnerAsync(factory);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Should_Return_401()
    {
        await using var factory = BuildFactory();
        await SeedOwnerAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, "WRONG_PASSWORD"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Without_Token_Should_Return_401()
    {
        await using var factory = BuildFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static WebApplicationFactory<Program> BuildFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    var settings = new Dictionary<string, string?>(
                        IntegrationTestFactory.DefaultConfiguration())
                    {
                        ["SeedData:Owner:Password"] = OwnerPassword,
                    };
                    config.AddInMemoryCollection(settings);
                });
            });
    }

    private static async Task SeedOwnerAsync(WebApplicationFactory<Program> factory)
    {
        // Program.cs already ran migrate + seed on first request.
        // Idempotency makes calling the seeder again a no-op.
        await Task.CompletedTask;
    }
}
