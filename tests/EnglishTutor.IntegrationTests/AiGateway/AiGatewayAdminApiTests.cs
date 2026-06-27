using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.AiGateway;

// HTTP-level admin API tests: authorization, masked-secret responses, and
// ProblemDetails error shapes. Authenticates as the seeded Owner account.
[Collection("EnglishTutorIntegrationTests")]
public class AiGatewayAdminApiTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private sealed class AiGatewayApiFactory : IntegrationTestFactory
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
                    ["Auth:RateLimit:Enabled"] = "false",
                    ["AiGateway:EncryptionMasterKey"] = "integration-test-ai-gateway-master-key-32+characters",
                });
            });
        }
    }

    [Fact]
    public async Task ProviderKey_Endpoint_Should_Mask_Secret_And_Require_Owner()
    {
        await using var factory = new AiGatewayApiFactory();
        var client = await LoginOwnerAsync(factory);

        var createProvider = await client.PostAsJsonAsync("/api/admin/ai-gateway/providers",
            new { Name = "Mock", Code = "mock", IsActive = true });
        createProvider.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await createProvider.Content.ReadApiDataAsync<IdResponse>();

        var createKey = await client.PostAsJsonAsync("/api/admin/ai-gateway/provider-keys",
            new { ProviderId = provider!.Id, Name = "primary", Secret = "sk-supersecret-7777", Priority = 0, IsActive = true });
        createKey.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync($"/api/admin/ai-gateway/provider-keys?providerId={provider.Id}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rawJson = await listResponse.Content.ReadAsStringAsync();

        rawJson.Should().NotContain("sk-supersecret-7777", "the raw secret must never be returned");
        rawJson.Should().NotContain("encryptedKey", "the encrypted secret must never be returned");
        rawJson.Should().Contain("****7777", "only the masked secret may be returned");
    }

    [Fact]
    public async Task Admin_Endpoints_Should_Require_Authentication()
    {
        await using var factory = new AiGatewayApiFactory();
        using var anonymous = factory.CreateClient();

        var response = await anonymous.GetAsync("/api/admin/ai-gateway/providers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Unknown_Provider_Should_Return_ProblemDetails_404()
    {
        await using var factory = new AiGatewayApiFactory();
        var client = await LoginOwnerAsync(factory);

        var response = await client.GetAsync($"/api/admin/ai-gateway/providers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("AiGateway.NotFound");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Create_Duplicate_Provider_Code_Should_Return_409()
    {
        await using var factory = new AiGatewayApiFactory();
        var client = await LoginOwnerAsync(factory);

        var first = await client.PostAsJsonAsync("/api/admin/ai-gateway/providers",
            new { Name = "OpenAI", Code = "dupe-code", IsActive = true });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/admin/ai-gateway/providers",
            new { Name = "OpenAI 2", Code = "dupe-code", IsActive = true });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await second.Content.ReadAsStringAsync()).Should().Contain("AiGateway.Conflict");
    }

    private sealed record IdResponse(Guid Id);

    private static async Task<HttpClient> LoginOwnerAsync(IntegrationTestFactory factory)
    {
        // The API host only migrates Identity/Audit on startup; ensure the
        // AiGateway schema exists before exercising its admin endpoints.
        using (var scope = factory.Services.CreateScope())
        {
            var aiDb = scope.ServiceProvider
                .GetRequiredService<EnglishTutor.AiGateway.Infrastructure.Persistence.AiGatewayDbContext>();
            await aiDb.Database.MigrateAsync();
        }

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.Content.ReadApiDataAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.AccessToken);
        return client;
    }
}
