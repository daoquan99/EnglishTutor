using System.Net;
using System.Net.Http.Json;
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

public class SecurityEventAuditTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private sealed class AuditTestFactory : IntegrationTestFactory
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
    public async Task Login_Refresh_Refresh_With_Consumed_Token_Should_Revoke_Family_And_Record_Reuse()
    {
        // Arrange
        await using var factory = new AuditTestFactory();
        using var client = factory.CreateClient();

        // Clean database tables from previous runs
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

        // 1. Login
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginPayload.Should().NotBeNull();
        var firstRefreshToken = loginPayload!.RefreshToken;

        // 2. First Refresh (consumes the first refresh token and rotates)
        var firstRefreshResponse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(firstRefreshToken));
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstRefreshPayload = await firstRefreshResponse.Content.ReadFromJsonAsync<RefreshResponse>();
        firstRefreshPayload.Should().NotBeNull();
        firstRefreshPayload!.RefreshToken.Should().NotBe(firstRefreshToken);

        // 3. Second Refresh with the consumed first refresh token (theft/reuse attempt)
        var secondRefreshResponse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(firstRefreshToken));
        
        // Assert: should return 401 Unauthorized
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 4. Verify that a SecurityEvent row is written to the Audit Database
        using var scope = factory.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var securityEvents = await auditDb.SecurityEvents
            .AsNoTracking()
            .Where(e => e.CategoryCode == AuditCategoryCodes.IdentityRefreshTokenReuseDetected)
            .ToListAsync();

        securityEvents.Should().ContainSingle();
        
        var auditRecord = securityEvents[0];
        auditRecord.SourceModule.Should().Be(AuditCategoryCodes.SourceModuleIdentity);
        auditRecord.ReasonCode.Should().Be("refresh_token_reuse");
        auditRecord.UserId.Should().NotBeNull().And.NotBe(Guid.Empty);
        auditRecord.RefreshTokenFamilyId.Should().NotBeNull().And.NotBe(Guid.Empty);
        auditRecord.RefreshTokenId.Should().NotBeNull().And.NotBe(Guid.Empty);
    }
}
