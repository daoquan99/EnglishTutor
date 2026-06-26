using System.Net;
using System.Net.Http.Json;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Audit;

[Collection("EnglishTutorIntegrationTests")]
public sealed class DurableSecurityEventConsumptionTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = "owner-test-password";

    private const string RefreshCookieName = "__Host-et_refresh";
    private const string CsrfCookieName = "__Host-et_csrf";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string CsrfToken = "test-csrf-token-value";

    private sealed class DurableFactory : IntegrationTestFactory, IAsyncDisposable
    {
        public DurableFactory()
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

        // Stop the in-memory buses BEFORE the host (and its database) go away,
        // so no consumer endpoint from this host survives to steal the next
        // test's published message. WebApplicationFactory.DisposeAsync does not
        // guarantee this ordering, so do it explicitly first.
        public override async ValueTask DisposeAsync()
        {
            foreach (var bus in Services.GetServices<MassTransit.IBusControl>())
            {
                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await bus.StopAsync(cts.Token);
                }
                catch
                {
                    // Best-effort: never let bus teardown mask a test assertion.
                }
            }

            Environment.SetEnvironmentVariable("Messaging__InProcessAuditConsumer", null);
            await base.DisposeAsync();
        }
    }

    [Fact]
    public async Task Login_Should_Persist_LoginSucceeded_SecurityEvent_Durably()
    {
        await using var factory = new DurableFactory();
        using var client = factory.CreateClient();
        await DurableSecurityEventTestSupport.ResetSecurityTablesAsync(factory);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var rows = await DurableSecurityEventTestSupport.PollSecurityEventsAsync(
            factory,
            e => e.CategoryCode == AuditCategoryCodes.IdentityLoginSucceeded,
            expectedCount: 1);

        rows.Should().NotBeEmpty("login success must produce a durable LoginSucceeded security event");
        rows[0].UserId.Should().NotBeNull().And.NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Refresh_Reuse_Should_Persist_Reuse_SecurityEvent_Durably_And_Idempotently()
    {
        await using var factory = new DurableFactory();
        using var client = factory.CreateClient();
        await DurableSecurityEventTestSupport.ResetSecurityTablesAsync(factory);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstToken = DurableSecurityEventTestSupport.ExtractCookie(login, RefreshCookieName);
        firstToken.Should().NotBeNullOrWhiteSpace();

        var firstRefresh = await client.SendAsync(RefreshWith(firstToken!));
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuse = await client.SendAsync(RefreshWith(firstToken!));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var rows = await DurableSecurityEventTestSupport.PollSecurityEventsAsync(
            factory,
            e => e.CategoryCode == AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            expectedCount: 1);

        rows.Should().ContainSingle("the durable reuse event must be recorded exactly once (inbox idempotency)");
        var row = rows[0];
        row.SourceModule.Should().Be(AuditCategoryCodes.SourceModuleIdentity);
        row.ReasonCode.Should().Be("refresh_token_reuse");
        row.UserId.Should().NotBeNull().And.NotBe(Guid.Empty);
        row.RefreshTokenFamilyId.Should().NotBeNull().And.NotBe(Guid.Empty);
        row.RefreshTokenId.Should().NotBeNull().And.NotBe(Guid.Empty);
    }

    private static HttpRequestMessage RefreshWith(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}; {CsrfCookieName}={CsrfToken}");
        request.Headers.Add(CsrfHeaderName, CsrfToken);
        return request;
    }
}
