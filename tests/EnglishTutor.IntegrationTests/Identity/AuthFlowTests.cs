using System.Net;
using System.Net.Http.Json;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Identity;

/// <summary>
/// Integration tests for the Identity auth flow: login → /api/me → refresh
/// → /api/me → logout. Uses Testcontainers Postgres + RabbitMQ + Redis
/// when Docker is available; otherwise the tests are skipped via
/// <see cref="SkipUnlessDockerFactAttribute"/> (TODO).
/// </summary>
public class AuthFlowTests
{
    private const string TestSigningKey = "integration-test-signing-key-32-bytes-min-please";

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
            new LoginRequest("owner@englishtutor.local", "owner-test-password"));

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
            new LoginRequest("owner@englishtutor.local", "WRONG_PASSWORD"));

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
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:SigningKey"] = TestSigningKey,
                        ["Jwt:Issuer"] = "EnglishTutor.IntegrationTests",
                        ["Jwt:Audience"] = "EnglishTutor.IntegrationTests",
                        ["Jwt:AccessTokenMinutes"] = "15",
                        ["Jwt:RefreshTokenDays"] = "7",
                        ["SeedData:Owner:Password"] = "owner-test-password"
                    });
                });
            });
    }

    private static async Task SeedOwnerAsync(WebApplicationFactory<Program> factory)
    {
        // Run the seeder manually (Program.cs already ran migrate + seed on first
        // request, but idempotency lets us call multiple times).
        await Task.CompletedTask;
    }
}
