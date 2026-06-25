using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Identity;

// Batch R1 H-04 / H-06 negative-path coverage: a user deactivated between
// login and refresh must not refresh, and the rate limiter returns a stable
// 429 ProblemDetails. Kept separate so the rate-limit factory can enable the
// limiter without affecting the functional AuthFlowTests.
[Collection("EnglishTutorIntegrationTests")]
public class AuthSecurityContainmentTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private const string RefreshCookieName = "__Host-et_refresh";
    private const string CsrfCookieName = "__Host-et_csrf";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string CsrfToken = "test-csrf-token-value";

    private sealed class RateLimitDisabledFactory : IntegrationTestFactory
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
                });
            });
        }
    }

    // A factory with a deliberately tiny login limit to exercise the 429 path.
    private sealed class LoginRateLimitFactory : IntegrationTestFactory
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
                    ["Auth:RateLimit:Enabled"] = "true",
                    ["Auth:RateLimit:Login:PermitLimit"] = "3",
                    ["Auth:RateLimit:Login:WindowSeconds"] = "60",
                });
            });
        }
    }

    [Fact]
    public async Task Refresh_With_Deactivated_User_Should_Be_Rejected_And_Not_Rotate()
    {
        await using var factory = new RateLimitDisabledFactory();
        var (client, refreshToken) = await LoginOwnerAsync(factory);

        // Deactivate the Owner directly via the repository (simulates an admin
        // disabling the account between login and refresh).
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IIdentityUnitOfWork>();
            var owner = await users.GetByEmailAsync(
                EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects.Email.Create(OwnerEmail),
                includeDeleted: false,
                default);
            owner.Should().NotBeNull();
            owner!.Deactivate();
            await uow.SaveChangesAsync(default);
        }

        // Refresh must be rejected (generic 401) and the cookie cleared.
        var response = await client.SendAsync(RefreshRequest(refreshToken));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The original token must NOT have rotated: a second attempt is still
        // rejected (no new valid token was minted for the disabled user).
        var second = await client.SendAsync(RefreshRequest(refreshToken));
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Over_Rate_Limit_Should_Return_429_ProblemDetails()
    {
        await using var factory = new LoginRateLimitFactory();
        using var client = factory.CreateClient();

        // Permit limit is 3 in a 60s window. Fire enough wrong-password logins
        // to exceed it; at least one response must be 429.
        HttpResponseMessage? limited = null;
        for (var i = 0; i < 8; i++)
        {
            var resp = await client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(OwnerEmail, "wrong-password"));
            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                limited = resp;
                break;
            }
        }

        limited.Should().NotBeNull("the limiter should reject once the window permit is exhausted");
        var body = await limited!.Content.ReadAsStringAsync();
        body.Should().Contain("auth.rate_limited");
        limited.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    // ---- helpers ----

    private static HttpRequestMessage RefreshRequest(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}; {CsrfCookieName}={CsrfToken}");
        request.Headers.Add(CsrfHeaderName, CsrfToken);
        return request;
    }

    private static async Task<(HttpClient Client, string RefreshToken)> LoginOwnerAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();

        var refreshToken = ExtractCookieValue(response, RefreshCookieName);
        refreshToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload!.AccessToken);

        return (client, refreshToken!);
    }

    private static string? ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;
        var raw = cookies.FirstOrDefault(c => c.StartsWith(cookieName + "=", StringComparison.Ordinal));
        if (raw is null) return null;
        var value = raw.Substring(cookieName.Length + 1);
        var semi = value.IndexOf(';');
        return semi >= 0 ? value.Substring(0, semi) : value;
    }
}
