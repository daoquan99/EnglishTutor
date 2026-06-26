using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Audit;

[Collection("EnglishTutorIntegrationTests")]
public class SecurityEventAuditTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private const string RefreshCookieName = "__Host-et_refresh";
    private const string CsrfCookieName = "__Host-et_csrf";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string CsrfToken = "test-csrf-token-value";

    private sealed class AuditTestFactory : IntegrationTestFactory
    {
        // Batch R1, H-07: run the durable pipeline in-process so the reuse
        // event flows Identity outbox -> delivery -> Audit consumer -> store.
        public AuditTestFactory()
        {
            Environment.SetEnvironmentVariable("Messaging__InProcessAuditConsumer", "true");
        }

        protected override bool KeepMessagingHostedServices => true;

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
                    ["Messaging:InProcessAuditConsumer"] = "true",
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Environment.SetEnvironmentVariable("Messaging__InProcessAuditConsumer", null);
            }
            base.Dispose(disposing);
        }
    }

    [Fact]
    public async Task Login_Refresh_Refresh_With_Consumed_Token_Should_Revoke_Family_And_Record_Reuse()
    {
        await using var factory = new AuditTestFactory();
        using var client = factory.CreateClient();

        // Clean tables from previous runs.
        using (var setupScope = factory.Services.CreateScope())
        {
            var setupAuditDb = setupScope.ServiceProvider.GetRequiredService<AuditDbContext>();
            var setupIdentityDb = setupScope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            setupAuditDb.SecurityEvents.RemoveRange(setupAuditDb.SecurityEvents);
            await setupAuditDb.SaveChangesAsync();

            setupIdentityDb.RefreshTokens.RemoveRange(setupIdentityDb.RefreshTokens);
            setupIdentityDb.RefreshTokenFamilies.RemoveRange(setupIdentityDb.RefreshTokenFamilies);
            setupIdentityDb.UserSessions.RemoveRange(setupIdentityDb.UserSessions);
            await setupIdentityDb.SaveChangesAsync();
        }

        // 1. Login -> extract refresh token from the __Host- cookie (H-01: not in body).
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstRefreshToken = ExtractCookieValue(loginResponse, RefreshCookieName);
        firstRefreshToken.Should().NotBeNullOrWhiteSpace();

        // 2. First refresh (cookie + CSRF) consumes + rotates the first token.
        var firstRefreshResponse = await client.SendAsync(
            RefreshRequest(firstRefreshToken!));
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondRefreshToken = ExtractCookieValue(firstRefreshResponse, RefreshCookieName);
        secondRefreshToken.Should().NotBeNullOrWhiteSpace().And.NotBe(firstRefreshToken);

        // 3. Second refresh reusing the consumed first token (theft/reuse).
        var secondRefreshResponse = await client.SendAsync(
            RefreshRequest(firstRefreshToken!));
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 4. The reuse security event is recorded durably in the Audit store.
        //    Delivery is async (outbox -> consumer), so poll with a bound.
        List<EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent> securityEvents = new();
        for (var i = 0; i < 60; i++)
        {
            using var pollScope = factory.Services.CreateScope();
            var pollDb = pollScope.ServiceProvider.GetRequiredService<AuditDbContext>();
            securityEvents = await pollDb.SecurityEvents
                .AsNoTracking()
                .Where(e => e.CategoryCode == AuditCategoryCodes.IdentityRefreshTokenReuseDetected)
                .ToListAsync();
            if (securityEvents.Count >= 1) break;
            await Task.Delay(250);
        }

        securityEvents.Should().ContainSingle();

        var auditRecord = securityEvents[0];
        auditRecord.SourceModule.Should().Be(AuditCategoryCodes.SourceModuleIdentity);
        auditRecord.ReasonCode.Should().Be("refresh_token_reuse");
        auditRecord.UserId.Should().NotBeNull().And.NotBe(Guid.Empty);
        auditRecord.RefreshTokenFamilyId.Should().NotBeNull().And.NotBe(Guid.Empty);
        auditRecord.RefreshTokenId.Should().NotBeNull().And.NotBe(Guid.Empty);
    }

    private static HttpRequestMessage RefreshRequest(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}; {CsrfCookieName}={CsrfToken}");
        request.Headers.Add(CsrfHeaderName, CsrfToken);
        return request;
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
