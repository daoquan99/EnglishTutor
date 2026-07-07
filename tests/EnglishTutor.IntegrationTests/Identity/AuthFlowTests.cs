using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Identity;

// All integration test classes share this xUnit collection so they run
// sequentially. See IntegrationTestFactory for the env-var-race rationale.
[Collection("EnglishTutorIntegrationTests")]
public class AuthFlowTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = IntegrationTestFactory.TestSeedOwnerPassword;

    // Secure __Host- cookies are not auto-sent by the test client over http,
    // so cookie-based flows are exercised by setting the Cookie header
    // manually. CSRF is stateless double-submit: cookie value == header value
    // validates, so a constant token is sufficient for the happy path.
    private const string RefreshCookieName = "__Host-et_refresh";
    private const string CsrfCookieName = "__Host-et_csrf";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string CsrfToken = "test-csrf-token-value";

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
                    // Disable rate limiting for the shared-IP functional tests;
                    // RateLimitingTests covers the 429 behavior explicitly.
                    ["Auth:RateLimit:Enabled"] = "false",
                });
            });
        }
    }

    [Fact]
    public async Task Login_With_Seeded_Owner_Account_Should_Not_Return_RefreshToken_In_Body()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rawJson = await response.Content.ReadAsStringAsync();
        // H-01: refresh token must NOT be in the JSON body.
        rawJson.Should().NotContain("refreshToken");
        rawJson.Should().NotContain("RefreshToken");

        var payload = await response.Content.ReadApiDataAsync<LoginResponse>();
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();

        // H-01/H-02: refresh token is delivered only via the __Host- cookie.
        ExtractRawSetCookie(response, RefreshCookieName).Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_Cookie_Should_Have_Host_Prefix_Secure_HttpOnly_SameSiteStrict_PathRoot_NoDomain()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var setCookie = ExtractRawSetCookie(login, RefreshCookieName);
        setCookie.Should().NotBeNull();
        var lower = setCookie!.ToLowerInvariant();

        setCookie.Should().StartWith("__Host-et_refresh=");
        lower.Should().Contain("path=/");
        lower.Should().Contain("secure");
        lower.Should().Contain("httponly");
        lower.Should().Contain("samesite=strict");
        lower.Should().NotContain("domain=");
    }

    [Fact]
    public async Task Login_RefreshCookieExpiry_Should_Use_RefreshTokenLifetime()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadApiDataAsync<LoginResponse>();
        var setCookie = ExtractRawSetCookie(response, RefreshCookieName);
        var cookieExpiresAt = ExtractCookieExpiry(setCookie!);

        cookieExpiresAt.Should().BeAfter(payload.ExpiresAt.AddDays(6));
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Should_Return_401()
    {
        await using var factory = new AuthFlowTestFactory();
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

    [Fact]
    public async Task GetSessions_Unauthenticated_Should_Return_401()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSessions_Authenticated_Should_Return_200_With_Sessions()
    {
        await using var factory = new AuthFlowTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/auth/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessions = await response.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        sessions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSessions_Should_Not_Leak_Sensitive_Token_Information()
    {
        await using var factory = new AuthFlowTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/auth/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("tokenHash");
        json.Should().NotContain("refreshToken");
        json.Should().NotContain("token_hash");
    }

    [Fact]
    public async Task RevokeSession_OwnSession_With_Csrf_Should_Succeed()
    {
        await using var factory = new AuthFlowTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var getResponse = await client.GetAsync("/api/auth/sessions");
        var sessions = await getResponse.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        var targetSessionId = sessions.First().Id;

        var deleteResponse = await client.SendAsync(
            Mutation(HttpMethod.Delete, $"/api/auth/sessions/{targetSessionId}"));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse2 = await client.GetAsync("/api/auth/sessions");
        var sessions2 = await getResponse2.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        sessions2.Should().NotContain(s => s.Id == targetSessionId);
    }

    [Fact]
    public async Task RevokeSession_Without_Csrf_Should_Return_403()
    {
        await using var factory = new AuthFlowTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var getResponse = await client.GetAsync("/api/auth/sessions");
        var sessions = await getResponse.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        var targetSessionId = sessions.First().Id;

        // No CSRF cookie/header -> 403.
        var deleteResponse = await client.DeleteAsync($"/api/auth/sessions/{targetSessionId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RevokeSession_OtherUserSession_Should_Return_404_For_StandardUser_But_Succeed_For_Admin()
    {
        await using var factory = new AuthFlowTestFactory();

        var userAEmail = "usera@test.local";
        var userBEmail = "userb@test.local";
        var userPassword = "password-123456";

        await CreateAndRegisterUserAsync(factory, userAEmail, userPassword, "User A");
        await CreateAndRegisterUserAsync(factory, userBEmail, userPassword, "User B");

        var clientA = factory.CreateClient();
        var loginResponseA = await clientA.PostAsJsonAsync("/api/auth/login", new LoginRequest(userAEmail, userPassword));
        var authA = await loginResponseA.Content.ReadApiDataAsync<LoginResponse>();
        clientA.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authA.AccessToken);

        var getSessionsA = await clientA.GetAsync("/api/auth/sessions");
        var sessionsA = await getSessionsA.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        var userASessionId = sessionsA.First().Id;

        var clientB = factory.CreateClient();
        var loginResponseB = await clientB.PostAsJsonAsync("/api/auth/login", new LoginRequest(userBEmail, userPassword));
        var authB = await loginResponseB.Content.ReadApiDataAsync<LoginResponse>();
        clientB.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authB.AccessToken);

        var deleteResponseByB = await clientB.SendAsync(
            Mutation(HttpMethod.Delete, $"/api/auth/sessions/{userASessionId}"));
        deleteResponseByB.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var (clientAdmin, _, _) = await LoginOwnerAsync(factory);
        var deleteResponseByAdmin = await clientAdmin.SendAsync(
            Mutation(HttpMethod.Delete, $"/api/auth/sessions/{userASessionId}"));
        deleteResponseByAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LogoutAll_With_Csrf_Should_Revoke_All_User_Sessions()
    {
        await using var factory = new AuthFlowTestFactory();

        var userEmail = "multi_session@test.local";
        var userPassword = "password-123456";
        await CreateAndRegisterUserAsync(factory, userEmail, userPassword, "Multi Device User");

        var client1 = factory.CreateClient();
        var loginRes1 = await client1.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));
        var auth1 = await loginRes1.Content.ReadApiDataAsync<LoginResponse>();

        var client2 = factory.CreateClient();
        await client2.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));

        client1.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth1.AccessToken);
        var logoutAllResponse = await client1.SendAsync(Mutation(HttpMethod.Post, "/api/auth/logout-all"));
        logoutAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getSessionsResponse = await client1.GetAsync("/api/auth/sessions");
        var activeSessions = await getSessionsResponse.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        activeSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Refresh_With_Revoked_Session_Should_Be_Blocked()
    {
        await using var factory = new AuthFlowTestFactory();
        var (client, _, refreshToken) = await LoginOwnerAsync(factory);

        var getSessions = await client.GetAsync("/api/auth/sessions");
        var sessions = await getSessions.Content.ReadApiDataAsync<List<UserSessionResponse>>();
        var sessionId = sessions.First().Id;

        var deleteResponse = await client.SendAsync(
            Mutation(HttpMethod.Delete, $"/api/auth/sessions/{sessionId}"));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Refresh with the (now revoked) session's cookie + valid CSRF -> 401.
        var refreshResponse = await client.SendAsync(
            Mutation(HttpMethod.Post, "/api/auth/refresh", refreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_Without_Cookie_Should_Return_401()
    {
        await using var factory = new AuthFlowTestFactory();
        using var client = factory.CreateClient();

        // No refresh cookie at all.
        var response = await client.PostAsync("/api/auth/refresh", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_With_Cookie_Missing_Csrf_Should_Return_403()
    {
        await using var factory = new AuthFlowTestFactory();
        var (_, _, refreshToken) = await LoginOwnerAsync(factory);
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}");
        // No CSRF cookie/header.
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Refresh_With_Mismatched_Csrf_Should_Return_403()
    {
        await using var factory = new AuthFlowTestFactory();
        var (_, _, refreshToken) = await LoginOwnerAsync(factory);
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}; {CsrfCookieName}=cookie-value");
        request.Headers.Add(CsrfHeaderName, "different-header-value");
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Refresh_With_Valid_Csrf_Should_Return_200_And_Not_Return_RefreshToken_In_Body()
    {
        await using var factory = new AuthFlowTestFactory();
        var (_, _, refreshToken) = await LoginOwnerAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.SendAsync(
            Mutation(HttpMethod.Post, "/api/auth/refresh", refreshToken));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rawJson = await response.Content.ReadAsStringAsync();
        rawJson.Should().NotContain("refreshToken");
        var setCookie = ExtractRawSetCookie(response, RefreshCookieName);
        setCookie.Should().NotBeNull();

        var payload = await response.Content.ReadApiDataAsync<RefreshResponse>();
        var cookieExpiresAt = ExtractCookieExpiry(setCookie!);
        cookieExpiresAt.Should().BeAfter(payload.ExpiresAt.AddDays(6));
    }

    // ---- helpers ----

    // Builds a mutation request carrying a valid double-submit CSRF pair
    // (cookie value == header value) plus an optional refresh cookie.
    private static HttpRequestMessage Mutation(HttpMethod method, string url, string? refreshToken = null)
    {
        var request = new HttpRequestMessage(method, url);
        var cookie = $"{CsrfCookieName}={CsrfToken}";
        if (refreshToken is not null)
        {
            cookie = $"{RefreshCookieName}={refreshToken}; " + cookie;
        }
        request.Headers.Add("Cookie", cookie);
        request.Headers.Add(CsrfHeaderName, CsrfToken);
        return request;
    }

    private static string? ExtractRawSetCookie(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;
        return cookies.FirstOrDefault(c => c.StartsWith(cookieName + "=", StringComparison.Ordinal));
    }

    private static string? ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        var raw = ExtractRawSetCookie(response, cookieName);
        if (raw is null) return null;
        var value = raw.Substring(cookieName.Length + 1);
        var semi = value.IndexOf(';');
        return semi >= 0 ? value.Substring(0, semi) : value;
    }

    private static DateTimeOffset ExtractCookieExpiry(string setCookie)
    {
        var expires = setCookie
            .Split(';', StringSplitOptions.TrimEntries)
            .Single(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));

        return DateTimeOffset.Parse(
            expires["expires=".Length..],
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);
    }

    private static async Task<(HttpClient Client, LoginResponse Auth, string RefreshToken)> LoginOwnerAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadApiDataAsync<LoginResponse>();

        var refreshToken = ExtractCookieValue(response, RefreshCookieName);
        refreshToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload.AccessToken);

        return (client, payload, refreshToken!);
    }

    private static async Task<string> CreateAndRegisterUserAsync(
        WebApplicationFactory<Program> factory,
        string email,
        string password,
        string displayName)
    {
        using var scope = factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IIdentityUnitOfWork>();

        var user = User.Create(
            Email.Create(email),
            HashedPassword.FromNewHash(passwordHasher.HashPassword(password)),
            displayName,
            roleIds: Array.Empty<Guid>(),
            createdByUserId: null);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(default);
        return email;
    }
}
