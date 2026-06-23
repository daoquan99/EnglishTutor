using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
// sequentially. The collection is needed because
// IntegrationTestFactory sets process-wide environment variables
// (ConnectionStrings__Default, ConnectionStrings__Audit,
// SeedData__Owner__Password, Database__ApplyAuditMigrationsOnStartup)
// in its constructor so the per-factory temporary database override
// reaches the DbContext registration lambdas. Running integration tests
// in parallel would race on those env vars and the factories would
// collide on the wrong database. The collection forces sequential
// execution; per-class isolation still works because each factory
// instance has its own _testDbName. See IntegrationTestFactory for the
// rationale and the env-var set.
[Collection("EnglishTutorIntegrationTests")]
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
        await SeedOwnerAsync(factory);

        var (client, auth, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/auth/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessions = await response.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        sessions.Should().NotBeNull();
        sessions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSessions_Should_Not_Leak_Sensitive_Token_Information()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        var (client, auth, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/auth/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("tokenHash");
        json.Should().NotContain("refreshToken");
        json.Should().NotContain("token_hash");
    }

    [Fact]
    public async Task RevokeSession_OwnSession_Should_Succeed()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        var (client, auth, _) = await LoginOwnerAsync(factory);

        var getResponse = await client.GetAsync("/api/auth/sessions");
        var sessions = await getResponse.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        var targetSessionId = sessions!.First().Id;

        var deleteResponse = await client.DeleteAsync($"/api/auth/sessions/{targetSessionId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify session is no longer returned as active
        var getResponse2 = await client.GetAsync("/api/auth/sessions");
        var sessions2 = await getResponse2.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        sessions2.Should().NotContain(s => s.Id == targetSessionId);
    }

    [Fact]
    public async Task RevokeSession_OtherUserSession_Should_Return_404_For_StandardUser_But_Succeed_For_Admin()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        // Create standard user A and user B
        var userAEmail = "usera@test.local";
        var userBEmail = "userb@test.local";
        var userPassword = "password-123456";

        await CreateAndRegisterUserAsync(factory, userAEmail, userPassword, "User A");
        await CreateAndRegisterUserAsync(factory, userBEmail, userPassword, "User B");

        // Log in as user A to get a session
        var clientA = factory.CreateClient();
        var loginResponseA = await clientA.PostAsJsonAsync("/api/auth/login", new LoginRequest(userAEmail, userPassword));
        var authA = await loginResponseA.Content.ReadFromJsonAsync<LoginResponse>();
        clientA.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authA!.AccessToken);

        var getSessionsA = await clientA.GetAsync("/api/auth/sessions");
        var sessionsA = await getSessionsA.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        var userASessionId = sessionsA!.First().Id;

        // Log in as user B and attempt to revoke user A's session
        var clientB = factory.CreateClient();
        var loginResponseB = await clientB.PostAsJsonAsync("/api/auth/login", new LoginRequest(userBEmail, userPassword));
        var authB = await loginResponseB.Content.ReadFromJsonAsync<LoginResponse>();
        clientB.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authB!.AccessToken);

        var deleteResponseByB = await clientB.DeleteAsync($"/api/auth/sessions/{userASessionId}");
        deleteResponseByB.StatusCode.Should().Be(HttpStatusCode.NotFound); // Returns NotFound to prevent user session enumeration

        // Log in as Owner (Admin) and revoke User A's session
        var (clientAdmin, _, _) = await LoginOwnerAsync(factory);
        var deleteResponseByAdmin = await clientAdmin.DeleteAsync($"/api/auth/sessions/{userASessionId}");
        deleteResponseByAdmin.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task LogoutAll_Should_Revoke_All_User_Sessions()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        // Standard user logs in twice (simulating different devices)
        var userEmail = "multi_session@test.local";
        var userPassword = "password-123456";
        await CreateAndRegisterUserAsync(factory, userEmail, userPassword, "Multi Device User");

        var client1 = factory.CreateClient();
        var loginRes1 = await client1.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));
        var auth1 = await loginRes1.Content.ReadFromJsonAsync<LoginResponse>();

        var client2 = factory.CreateClient();
        var loginRes2 = await client2.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));
        var auth2 = await loginRes2.Content.ReadFromJsonAsync<LoginResponse>();

        // Set Authorization header for client1 and perform logout-all
        client1.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth1!.AccessToken);
        var logoutAllResponse = await client1.PostAsync("/api/auth/logout-all", null);
        logoutAllResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify sessions are now empty
        var getSessionsResponse = await client1.GetAsync("/api/auth/sessions");
        var activeSessions = await getSessionsResponse.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        activeSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Refresh_With_Revoked_Session_Should_Be_Blocked()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        var (client, auth, _) = await LoginOwnerAsync(factory);

        // Get the active session ID
        var getSessions = await client.GetAsync("/api/auth/sessions");
        var sessions = await getSessions.Content.ReadFromJsonAsync<List<UserSessionResponse>>();
        var sessionId = sessions!.First().Id;

        // Revoke the session
        var deleteResponse = await client.DeleteAsync($"/api/auth/sessions/{sessionId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Attempt to refresh using the refresh token from that session
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_With_Cookie_Without_Csrf_Header_Should_Fail_With_400_But_Succeed_With_Csrf_Header()
    {
        await using var factory = new AuthFlowTestFactory();
        await SeedOwnerAsync(factory);

        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(OwnerEmail, OwnerPassword));
        var auth = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 1. Missing CSRF header should return 400 Bad Request
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request1.Headers.Add("Cookie", $"__Host-et_refresh={auth!.RefreshToken}");
        var response1 = await client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorMsg = await response1.Content.ReadAsStringAsync();
        errorMsg.Should().Contain("CSRF validation failed");

        // 2. Present approved CSRF header should succeed (200 OK)
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request2.Headers.Add("Cookie", $"__Host-et_refresh={auth.RefreshToken}");
        request2.Headers.Add("X-Requested-With", "XMLHttpRequest");
        var response2 = await client.SendAsync(request2);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task SeedOwnerAsync(WebApplicationFactory<Program> factory)
    {
        // Program.cs already ran migrate + seed on first request.
        // Idempotency makes calling the seeder again a no-op.
        await Task.CompletedTask;
    }

    private static async Task<(HttpClient Client, LoginResponse Auth, string RefreshCookie)> LoginOwnerAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();

        // Retrieve the cookie header if set
        string? refreshCookie = null;
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            refreshCookie = values.FirstOrDefault(v => v.StartsWith("__Host-et_refresh="));
        }

        // Set the Authorization header for all future requests using this client
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload!.AccessToken);

        return (client, payload, refreshCookie ?? "");
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
