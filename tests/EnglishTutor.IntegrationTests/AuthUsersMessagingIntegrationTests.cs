extern alias WorkerHost;

using System.Net;
using System.Net.Http.Json;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.IntegrationTests.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Auth.Presentation.Requests;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkerHost::EnglishTutor.Worker.Outbox;
using Xunit;

namespace EnglishTutor.IntegrationTests;

public sealed class AuthUsersMessagingIntegrationTests(DatabaseEnglishTutorApiFactory factory)
    : IClassFixture<DatabaseEnglishTutorApiFactory>
{
    [Fact]
    public async Task RegisterEndpoint_ShouldPersistAuthUserAndOutboxMessage()
    {
        if (!factory.Database.IsAvailable)
        {
            return;
        }

        using var client = factory.CreateClient();
        var email = $"learner-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            email,
            "Password123!",
            "Password123!",
            "Integration Learner"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var scope = factory.Services.CreateAsyncScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        (await authDbContext.AuthUsers.AnyAsync(user => user.Email.Value == email)).Should().BeTrue();
        var outboxMessage = await authDbContext.OutboxMessages.SingleAsync(message => message.SourceModule == "auth");
        outboxMessage.Status.Should().Be(OutboxMessageStatus.Pending);
        outboxMessage.Payload.Should().Contain(email);
    }

    [Fact]
    public async Task WorkerOutboxProcessing_ShouldProjectRegisteredUserAndRemainIdempotent()
    {
        if (!factory.Database.IsAvailable)
        {
            return;
        }

        using var client = factory.CreateClient();
        var email = $"worker-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            email,
            "Password123!",
            "Password123!",
            "Worker Learner"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var workerProvider = IntegrationWorkerHost.CreateServiceProvider(factory.Database.ConnectionString);
        await using (var workerScope = workerProvider.CreateAsyncScope())
        {
            await workerScope.ServiceProvider.GetRequiredService<MessagingDbContext>().Database.MigrateAsync();
            var processor = workerScope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            await processor.ProcessPendingMessagesAsync();
            await processor.ProcessPendingMessagesAsync();
        }

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var authDbContext = assertionScope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var usersDbContext = assertionScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var authUser = await authDbContext.AuthUsers.SingleAsync(user => user.Email.Value == email);
        var outboxMessage = await authDbContext.OutboxMessages.SingleAsync(message => message.SourceModule == "auth" && message.Status == OutboxMessageStatus.Processed);

        (await usersDbContext.UserProfiles.CountAsync(profile => profile.UserId == authUser.Id)).Should().Be(1);
        (await usersDbContext.UserLanguageSettings.CountAsync(settings => settings.UserId == authUser.Id)).Should().Be(1);
        (await usersDbContext.UserTargetLanguages.CountAsync(language => language.UserId == authUser.Id)).Should().Be(1);
        (await usersDbContext.InboxMessages.CountAsync(message => message.EventId == outboxMessage.EventId)).Should().Be(1);
    }

    [Fact]
    public async Task WorkerOutboxProcessing_ShouldRecoverLockedProcessingMessages_WhenLeaseExpires()
    {
        if (!factory.Database.IsAvailable)
        {
            return;
        }

        // 1. Register a user to create a pending outbox message
        using var client = factory.CreateClient();
        var email = $"lease-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            email,
            "Password123!",
            "Password123!",
            "Lease Learner"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Fetch that specific outbox message using the email payload, transition it to "Processing" and lock it in the past (expired lock)
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var outboxMessage = await authDbContext.OutboxMessages.SingleAsync(message => message.SourceModule == "auth" && message.Payload.Contains(email));
            
            outboxMessage.Status = OutboxMessageStatus.Processing;
            outboxMessage.LockedBy = "StuckWorker";
            outboxMessage.LockedUntilUtc = DateTime.UtcNow.AddMinutes(-5); // Expired lock
            outboxMessage.NextRetryAtUtc = DateTime.UtcNow.AddMinutes(-5); // Expired retry at
            
            await authDbContext.SaveChangesAsync();
        }

        // 3. Run outbox processor and verify it recovers the message and completes it successfully
        await using var workerProvider = IntegrationWorkerHost.CreateServiceProvider(factory.Database.ConnectionString);
        await using (var workerScope = workerProvider.CreateAsyncScope())
        {
            await workerScope.ServiceProvider.GetRequiredService<MessagingDbContext>().Database.MigrateAsync();
            var processor = workerScope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            await processor.ProcessPendingMessagesAsync();
        }

        // 4. Verify message is Processed and locked by the new processor, not "StuckWorker"
        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionAuthDbContext = assertionScope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var finalOutboxMessage = await assertionAuthDbContext.OutboxMessages.SingleAsync(message => message.SourceModule == "auth" && message.Payload.Contains(email));
        
        finalOutboxMessage.Status.Should().Be(OutboxMessageStatus.Processed);
        finalOutboxMessage.LockedBy.Should().NotBe("StuckWorker");
    }
}
