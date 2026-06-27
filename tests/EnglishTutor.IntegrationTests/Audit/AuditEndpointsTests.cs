using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Audit.Application.Queries.SearchAuditLogs;
using EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;
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

namespace EnglishTutor.IntegrationTests.Audit;

[Collection("EnglishTutorIntegrationTests")]
public class AuditEndpointsTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private sealed class AuditEndpointsTestFactory : IntegrationTestFactory
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
    public async Task GetAuditLogs_Unauthenticated_Should_Return_401()
    {
        await using var factory = new AuditEndpointsTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/audit/logs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSecurityEvents_Unauthenticated_Should_Return_401()
    {
        await using var factory = new AuditEndpointsTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/audit/security-events");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_As_StandardUser_Should_Return_403()
    {
        await using var factory = new AuditEndpointsTestFactory();
        var client = factory.CreateClient();

        var userEmail = "standard@test.local";
        var userPassword = "password-123456";
        await CreateAndRegisterUserAsync(factory, userEmail, userPassword, "Standard User");

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));
        var loginPayload = await loginResponse.Content.ReadApiDataAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var response = await client.GetAsync("/api/admin/audit/logs");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSecurityEvents_As_StandardUser_Should_Return_403()
    {
        await using var factory = new AuditEndpointsTestFactory();
        var client = factory.CreateClient();

        var userEmail = "standard2@test.local";
        var userPassword = "password-123456";
        await CreateAndRegisterUserAsync(factory, userEmail, userPassword, "Standard User 2");

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(userEmail, userPassword));
        var loginPayload = await loginResponse.Content.ReadApiDataAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var response = await client.GetAsync("/api/admin/audit/security-events");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_As_Owner_Should_Return_200()
    {
        await using var factory = new AuditEndpointsTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/admin/audit/logs?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadApiResponseAsync<List<AuditLogResponseDto>>();
        pagedResult.Meta!.Pagination!.Page.Should().Be(1);
        pagedResult.Meta.Pagination.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetSecurityEvents_As_Owner_Should_Return_200()
    {
        await using var factory = new AuditEndpointsTestFactory();
        var (client, _, _) = await LoginOwnerAsync(factory);

        var response = await client.GetAsync("/api/admin/audit/security-events?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadApiResponseAsync<List<SecurityEventResponseDto>>();
        pagedResult.Meta!.Pagination!.Page.Should().Be(1);
        pagedResult.Meta.Pagination.PageSize.Should().Be(10);
    }

    private static async Task<(HttpClient Client, LoginResponse Auth, string RefreshCookie)> LoginOwnerAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var payload = await response.Content.ReadApiDataAsync<LoginResponse>();

        string? refreshCookie = null;
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            refreshCookie = values.FirstOrDefault(v => v.StartsWith("__Host-et_refresh="));
        }

        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload.AccessToken);

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

        var user = EnglishTutor.Identity.Domain.Aggregates.Users.User.Create(
            EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects.Email.Create(email),
            EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects.HashedPassword.FromNewHash(passwordHasher.HashPassword(password)),
            displayName,
            roleIds: Array.Empty<Guid>(),
            createdByUserId: null);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(default);
        return email;
    }
}
