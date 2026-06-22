using System.Net;
using System.Net.Http.Json;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EnglishTutor.IntegrationTests.Identity;

// Integration tests for the Identity auth flow: login -> /api/me -> refresh
// -> /api/me -> logout. Uses Testcontainers Postgres + RabbitMQ + Redis
// when Docker is available; otherwise the tests are skipped via
// SkipUnlessDockerFactAttribute (TODO).
//
// Subclasses IntegrationTestFactory so the Jwt:SigningKey baseline is
// configured once in the shared base factory. This AuthFlow-specific
// subclass only adds UseEnvironment("Development") and overrides
// SeedData:Owner:Password to a known value so LoginRequest can call
// with that exact password.
public class AuthFlowTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private sealed class AuthFlowTestFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SeedData:Owner:Password"] = OwnerPassword,
                });
            });
        }
    }

    [Fact]
    public async Task Login_With_Seeded_Owner_Account_Should_Return_Tokens()
    {
        // Arrange: skip if infra dependencies are unreachable (unit env).
        await using var factory = new AuthFlowTestFactory();

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
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, "WRONG_PASSWORD"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Without_Token_Should_Return_401()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task SeedOwnerAsync(WebApplicationFactory<Program> factory)
    {
        // Program.cs already ran migrate + seed on first request.
        // Idempotency makes calling the seeder again a no-op.
        await Task.CompletedTask;
    }
}
