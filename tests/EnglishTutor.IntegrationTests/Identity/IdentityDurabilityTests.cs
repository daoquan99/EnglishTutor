using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace EnglishTutor.IntegrationTests.Identity;

[Collection("EnglishTutorIntegrationTests")]
public class IdentityDurabilityTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = IntegrationTestFactory.TestSeedOwnerPassword;

    private sealed class FailingPublisherFactory : IntegrationTestFactory
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
            builder.ConfigureServices(services =>
            {
                var desc = services.FirstOrDefault(d => d.ServiceType == typeof(IIdentitySecurityEventPublisher));
                if (desc != null)
                {
                    services.Remove(desc);
                }

                var mock = Substitute.For<IIdentitySecurityEventPublisher>();
                mock.PublishAsync(Arg.Any<IdentitySecurityEventData>(), Arg.Any<CancellationToken>())
                    .Returns(x => throw new InvalidOperationException("Forced publisher outbox failure"));

                services.AddScoped<IIdentitySecurityEventPublisher>(_ => mock);
            });
        }
    }

    private sealed class HappyPathFactory : IntegrationTestFactory
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

    [Fact]
    public async Task Login_WhenPublisherOutboxFails_ShouldRollbackAndNotCreateSession()
    {
        // Arrange
        await using var factory = new FailingPublisherFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Email.Value == OwnerEmail);
        owner.Should().NotBeNull();

        var sessions = await db.UserSessions.Where(s => s.UserId == owner!.Id).ToListAsync();
        sessions.Should().BeEmpty("the session creation must be rolled back on outbox staging failure");
    }

    [Fact]
    public async Task Login_HappyPath_ShouldCreateSessionAndStageOutboxMessage()
    {
        // Arrange
        await using var factory = new HappyPathFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Email.Value == OwnerEmail);
        owner.Should().NotBeNull();

        var sessions = await db.UserSessions.Where(s => s.UserId == owner!.Id).ToListAsync();
        sessions.Should().NotBeEmpty("session should be created on happy path");

        var outboxMessages = await db.OutboxMessages.ToListAsync();
        outboxMessages.Should().NotBeEmpty("outbox messages must contain recorded security event");
    }
}
