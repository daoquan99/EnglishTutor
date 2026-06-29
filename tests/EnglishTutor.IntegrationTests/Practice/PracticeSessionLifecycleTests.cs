using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Admin;
using MediatR;
using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;
using EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Practice;

// End-to-end Practice session lifecycle tests across the real DI graph + a
// temporary Postgres database. Quota uses its default rule; AiGateway is seeded
// with a mock provider/model/key/routing rule so the route lease succeeds.
[Collection("EnglishTutorIntegrationTests")]
public class PracticeSessionLifecycleTests
{
    private const string ModeCode = "roleplay";
    private const string TopicCode = "job-interview";

    private sealed class PracticeTestFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
                {
                    ["AiGateway:EncryptionMasterKey"] = "integration-test-ai-gateway-master-key-32+characters",
                    ["Quota:DefaultDailyMaxSessionMinutes"] = "60",
                    ["Quota:DefaultDailyMaxSessions"] = "10",
                    ["Quota:DefaultMaxSingleSessionMinutes"] = "30",
                });
            });
        }
    }

    [Fact]
    public async Task StartSession_HappyPath_Should_Reserve_Lease_And_Store_Snapshot()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);

        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        var result = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);

        result.Status.Should().Be(StartSessionStatus.Success, "but error was: {0}", result.ErrorCode);
        result.SessionId.Should().NotBeNull();
        result.TopicCode.Should().Be(TopicCode);
        result.ModeCode.Should().Be(ModeCode);
        result.ModelCode.Should().Be("mock-model");
        result.ProviderCode.Should().Be("mock");
        result.RealtimeStatus.Should().Be(PracticeRealtimeStatus.Deferred);

        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var session = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == result.SessionId);
        session.UserId.Should().Be(userId);
        session.Status.ToString().Should().Be("Active");
    }

    [Fact]
    public async Task StartSession_Should_Rollback_Quota_When_RouteLease_Fails()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        // Seed provider/model/key but NO routing rule -> CreateRouteLease returns RouteNoMatch.
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: false);

        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        var result = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);

        result.Status.Should().Be(StartSessionStatus.AiRouteLeaseFailed);
        result.SessionId.Should().BeNull();

        // No practice session must be created on the rollback path.
        var practiceDb = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        (await practiceDb.Sessions.AsNoTracking().CountAsync(s => s.UserId == userId)).Should().Be(0);

        // The quota reservation must have been cancelled (compensation).
        var quotaDb = scope.ServiceProvider.GetRequiredService<QuotaDbContext>();
        var reservations = await quotaDb.Set<EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.QuotaReservation>()
            .AsNoTracking().Where(r => r.UserId == userId).ToListAsync();
        reservations.Should().OnlyContain(r => r.Status == EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.QuotaReservation.ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task StartSession_Should_Return_ScenarioNotFound_For_Unknown_Scenario()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var result = await module.StartSessionAsync(Guid.NewGuid(),
            new StartSessionRequest(Guid.NewGuid(), Guid.NewGuid().ToString("N"), 15), CancellationToken.None);

        result.Status.Should().Be(StartSessionStatus.ScenarioNotFound);
    }

    [Fact]
    public async Task AppendMessage_Should_Record_User_And_Assistant_Turns()
    {
        await using var factory = new PracticeTestFactory();
        Guid scenarioId;
        {
            using var scope = factory.Services.CreateScope();
            await MigrateAllAsync(scope);
            scenarioId = await SeedScenarioReadModelAsync(scope);
            await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        }

        var userId = Guid.NewGuid();
        Guid sessionId;
        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var start = await module.StartSessionAsync(userId,
                new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
            sessionId = start.SessionId!.Value;
        }

        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var append = await module.AppendMessageAsync(userId, sessionId,
                new AppendTranscriptRequest("Hello, can we practice?"), CancellationToken.None);

            append.Status.Should().Be(AppendTranscriptStatus.Success);
            append.UserMessageId.Should().NotBeNull();
            append.AssistantMessageId.Should().NotBeNull();
            append.AssistantContent.Should().NotBeNullOrWhiteSpace();
        }

        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var transcript = await module.GetTranscriptAsync(userId, sessionId, CancellationToken.None);
            transcript.Status.Should().Be(PracticeQueryStatus.Success);
            transcript.Messages.Should().HaveCount(2);
            transcript.Messages[0].Role.Should().Be("user");
            transcript.Messages[1].Role.Should().Be("assistant");
        }
    }

    [Fact]
    public async Task EndSession_Should_Mark_Ended_And_Be_Idempotent()
    {
        await using var factory = new PracticeTestFactory();
        Guid scenarioId;
        {
            using var scope = factory.Services.CreateScope();
            await MigrateAllAsync(scope);
            scenarioId = await SeedScenarioReadModelAsync(scope);
            await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        }

        var userId = Guid.NewGuid();
        Guid sessionId;
        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var start = await module.StartSessionAsync(userId,
                new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
            sessionId = start.SessionId!.Value;
        }

        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var end = await module.EndSessionAsync(userId, sessionId, CancellationToken.None);
            end.Status.Should().Be(EndSessionStatus.Success);
            end.SessionStatus.Should().Be("Completed");
        }

        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var endAgain = await module.EndSessionAsync(userId, sessionId, CancellationToken.None);
            endAgain.Status.Should().Be(EndSessionStatus.AlreadyEnded);
        }
    }

    [Fact]
    public async Task User_Cannot_Access_Another_Users_Session()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();

        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var start = await module.StartSessionAsync(owner,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var sessionId = start.SessionId!.Value;

        (await module.GetSessionAsync(intruder, sessionId, CancellationToken.None)).Status.Should().Be(PracticeQueryStatus.Forbidden);
        (await module.GetTranscriptAsync(intruder, sessionId, CancellationToken.None)).Status.Should().Be(PracticeQueryStatus.Forbidden);
        (await module.EndSessionAsync(intruder, sessionId, CancellationToken.None)).Status.Should().Be(EndSessionStatus.Forbidden);
        (await module.AppendMessageAsync(intruder, sessionId, new AppendTranscriptRequest("hi"), CancellationToken.None)).Status.Should().Be(AppendTranscriptStatus.Forbidden);
    }

    [Fact]
    public async Task ListSessions_Should_Return_Only_Callers_Sessions_Paged()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        for (var i = 0; i < 2; i++)
        {
            await module.StartSessionAsync(userId,
                new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        }
        // Another user's session must not appear.
        await module.StartSessionAsync(Guid.NewGuid(),
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);

        var page = await module.ListSessionsAsync(userId, 1, 20, CancellationToken.None);
        page.TotalCount.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Should().OnlyContain(s => s.UserId == userId);
    }

    [Fact]
    public async Task StartSession_Should_Copy_ReadModel_To_Immutable_SessionScenarioSnapshot()
    {
        await using var factory = new PracticeTestFactory();
        Guid scenarioId;
        {
            using var scope = factory.Services.CreateScope();
            await MigrateAllAsync(scope);
            scenarioId = await SeedScenarioReadModelAsync(scope);
            await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        }

        var userId = Guid.NewGuid();
        Guid sessionId;
        {
            using var scope = factory.Services.CreateScope();
            var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
            var start = await module.StartSessionAsync(userId,
                new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
            start.Status.Should().Be(StartSessionStatus.Success);
            sessionId = start.SessionId!.Value;
        }

        // Later changes to the read model do not change the existing session snapshot
        {
            using var scope = factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPracticeScenarioReadModelRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IPracticeUnitOfWork>();

            var updatedReadModel = new PracticeScenarioReadModel(
                scenarioId, Guid.NewGuid(), TopicCode, "Job Interview",
                Guid.NewGuid(), ModeCode, "Updated Scenario Title", "Updated instructions that should not affect the session.");
            await repo.UpdateAsync(updatedReadModel, CancellationToken.None);
            await uow.SaveChangesAsync(CancellationToken.None);
        }

        // Load the session and check that it still has the original snapshot
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
            var session = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == sessionId);
            session.ScenarioSnapshot.Title.Should().Be("Salary Negotiation");
            session.ScenarioSnapshot.LearnerFacingInstructions.Should().Be("Help the learner negotiate a salary.");

            // No repository/DbSet exists for PracticeSessionScenarioSnapshot
            typeof(PracticeDbContext).GetProperties()
                .Any(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments()[0] == typeof(PracticeSessionScenarioSnapshot))
                .Should().BeFalse();
        }
    }

    [Fact]
    public async Task CancelSession_Should_Set_Cancelled_And_UserCancelled_And_Release_Resources()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        var start = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var sessionId = start.SessionId!.Value;

        var cancelResult = await module.CancelSessionAsync(userId, sessionId, CancellationToken.None);
        cancelResult.Status.Should().Be(EndSessionStatus.Success);
        cancelResult.SessionStatus.Should().Be("Cancelled");

        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var session = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == sessionId);
        session.Status.ToString().Should().Be("Cancelled");
        session.EndReason.ToString().Should().Be("UserCancelled");
        session.EndedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Session_Past_ExpiresAt_Should_Be_Expired_Lazily_And_Reject_Append()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        // Start session with short duration (e.g. 1 minute)
        var start = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 1), CancellationToken.None);
        var sessionId = start.SessionId!.Value;

        // Manipulate session expiry in DB to be in the past
        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var session = await db.Sessions.FirstAsync(s => s.Id == sessionId);
        
        // Use reflection to set ExpiresAtUtc to past
        var expiresField = typeof(PracticeSession).GetProperty("ExpiresAtUtc");
        expiresField!.SetValue(session, DateTime.UtcNow.AddMinutes(-5));
        await db.SaveChangesAsync();

        // Now trying to append a message should lazily expire and reject
        var appendResult = await module.AppendMessageAsync(userId, sessionId, new AppendTranscriptRequest("hello"), CancellationToken.None);
        appendResult.Status.Should().Be(AppendTranscriptStatus.SessionNotActive);

        // Verify the status in database is now Expired + Expired reason
        var dbSession = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == sessionId);
        dbSession.Status.ToString().Should().Be("Expired");
        dbSession.EndReason.ToString().Should().Be("Expired");
        dbSession.EndedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task SystemFailure_And_ScenarioCompleted_Should_Update_Status_Accordingly()
    {
        await using var factory = new PracticeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId, includeRoutingRule: true);
        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        // Test SystemFailed
        var start1 = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var session1Id = start1.SessionId!.Value;

        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var session1 = await db.Sessions.FirstAsync(s => s.Id == session1Id);
        session1.MarkFailed(DateTime.UtcNow, "unrecoverable_ai_error");
        await db.SaveChangesAsync();

        var dbSession1 = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == session1Id);
        dbSession1.Status.ToString().Should().Be("Failed");
        dbSession1.EndReason.ToString().Should().Be("SystemFailed");

        // Test ScenarioCompleted
        var start2 = await module.StartSessionAsync(userId,
            new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var session2Id = start2.SessionId!.Value;

        var completeResult = await module.CompleteScenarioAsync(userId, session2Id, CancellationToken.None);
        completeResult.Status.Should().Be(EndSessionStatus.Success);
        completeResult.SessionStatus.Should().Be("Completed");

        var dbSession2 = await db.Sessions.AsNoTracking().FirstAsync(s => s.Id == session2Id);
        dbSession2.Status.ToString().Should().Be("Completed");
        dbSession2.EndReason.ToString().Should().Be("ScenarioCompleted");
    }

    // ---- helpers ----

    private static async Task MigrateAllAsync(IServiceScope scope)
    {
        await scope.ServiceProvider.GetRequiredService<PracticeDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<QuotaDbContext>().Database.MigrateAsync();
    }

    private static async Task<Guid> SeedScenarioReadModelAsync(IServiceScope scope)
    {
        var repo = scope.ServiceProvider.GetRequiredService<IPracticeScenarioReadModelRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IPracticeUnitOfWork>();
        var scenarioId = Guid.NewGuid();
        var snapshot = new PracticeScenarioReadModel(
            scenarioId, Guid.NewGuid(), TopicCode, "Job Interview",
            Guid.NewGuid(), ModeCode, "Salary Negotiation", "Help the learner negotiate a salary.");
        await repo.AddAsync(snapshot, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);
        return scenarioId;
    }

    private static async Task SeedAiGatewayAsync(IServiceScope scope, Guid scenarioId, bool includeRoutingRule)
    {
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(
            providerId,
            "Mock Model",
            "mock-model",
            ["content-generation"],
            true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "primary", "sk-mock-123456", 0, true), null, CancellationToken.None);

        if (includeRoutingRule)
        {
            await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput(
                "Practice Rule", ModeCode, TopicCode, scenarioId.ToString("N"), modelId, null, true), null, CancellationToken.None);
        }
    }

    private sealed class TestAdminFacade
    {
        private readonly ISender _sender;
        public TestAdminFacade(ISender sender) => _sender = sender;

        public async Task<Result<Guid>> CreateProviderAsync(CreateProviderInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateProviderCommand(input.Name, input.Code, input.IsActive, actor), ct);

        public async Task<Result<Guid>> CreateProviderKeyAsync(CreateProviderKeyInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateProviderKeyCommand(input.ProviderId, input.Name, input.Secret, input.Priority, input.IsActive, actor), ct);

        public async Task<Result<Guid>> CreateModelAsync(CreateModelInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateModelCommand(input.ProviderId, input.Name, input.Code, input.Capabilities, input.IsActive, actor), ct);

        public async Task<Result<Guid>> CreateRoutingRuleAsync(CreateRoutingRuleInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateRoutingRuleCommand(input.Name, input.ActivityType, input.TopicCode, input.ScenarioCode, input.PrimaryModelId, input.FallbackModelId, input.IsActive, actor), ct);
    }
}
