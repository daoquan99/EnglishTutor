using System.Net;
using System.Net.Http.Json;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using EnglishTutor.Learning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EnglishTutor.IntegrationTests.Audit;

[Collection("EnglishTutorIntegrationTests")]
public class DurableOutboxStagingTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";
    private const string RefreshCookieName = "__Host-et_refresh";

    private sealed class OutboxStagingFactory : IntegrationTestFactory
    {
        protected override bool KeepMessagingHostedServices => false;

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

    [Fact]
    public async Task Login_Should_Stage_Identity_Outbox_Only()
    {
        await using var factory = new OutboxStagingFactory();
        using var client = factory.CreateClient();

        await DurableSecurityEventTestSupport.MigrateLearningDbAsync(factory);
        await DurableSecurityEventTestSupport.ResetSecurityTablesAsync(factory);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var identityMessages = await DurableSecurityEventTestSupport.PollOutboxMessagesAsync<IdentityDbContext>(
            factory,
            m => m.MessageType.Contains(nameof(EnglishTutor.Identity.Contracts.Events.IdentitySecurityEventRecordedV1)),
            minCount: 1);
        var learningCount = await DurableSecurityEventTestSupport.CountOutboxMessagesAsync<LearningDbContext>(
            factory,
            m => m.MessageType.Contains(nameof(EnglishTutor.Identity.Contracts.Events.IdentitySecurityEventRecordedV1)));

        identityMessages.Should().HaveCount(1);
        learningCount.Should().Be(0);
    }

    [Fact]
    public async Task Outbox_Payload_Should_Contain_No_Secrets()
    {
        await using var factory = new OutboxStagingFactory();
        using var client = factory.CreateClient();

        await DurableSecurityEventTestSupport.ResetSecurityTablesAsync(factory);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = DurableSecurityEventTestSupport.ExtractCookie(login, RefreshCookieName);

        var messageBodies = await DurableSecurityEventTestSupport.PollOutboxBodiesAsync(factory, minCount: 1);
        messageBodies.Should().NotBeEmpty();

        foreach (var body in messageBodies)
        {
            body.Should().NotContain(token!, "raw refresh token must never appear in the outbox payload");
            body.Should().NotContain(OwnerPassword, "passwords must never appear in the outbox payload");
            body.ToLowerInvariant().Should().NotContain("password");
            body.Should().NotContain("__Host-", "cookie material must never appear in the outbox payload");
        }
    }
}
