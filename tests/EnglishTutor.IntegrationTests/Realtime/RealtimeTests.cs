using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using MediatR;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Persistence;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Application.Connections.Commands.JoinPracticeSession;
using EnglishTutor.Realtime.Application.Connections.Commands.LeavePracticeSession;
using EnglishTutor.Realtime.Application.Connections.Commands.RegisterHeartbeat;
using EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptPartial;
using EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptFinal;
using EnglishTutor.Realtime.Presentation.Hubs;
using EnglishTutor.Realtime.Contracts.Events;
using EnglishTutor.AiGateway.Application.Admin;
using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;
using EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;

namespace EnglishTutor.IntegrationTests.Realtime;

[Collection("EnglishTutorIntegrationTests")]
public class RealtimeTests
{
    private const string TopicCode = "job-interview";
    private const string ModeCode = "roleplay";

    private sealed class RealtimeTestFactory : IntegrationTestFactory
    {
        public readonly TestRealtimeNotifier Notifier = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AiGateway:EncryptionMasterKey"] = "integration-test-ai-gateway-master-key-32+characters",
                    ["Quota:DefaultDailyMaxSessionMinutes"] = "60",
                    ["Quota:DefaultDailyMaxSessions"] = "10",
                    ["Quota:DefaultMaxSingleSessionMinutes"] = "30",
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace production notifier with the test spy
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRealtimeNotifier));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<IRealtimeNotifier>(Notifier);
            });
        }
    }

    public sealed class TestRealtimeNotifier : IRealtimeNotifier
    {
        public List<object> Notifications { get; } = new();

        public Task NotifySessionStartedAsync(Guid sessionId, Guid? correlationId, string topicCode, string scenarioCode, CancellationToken ct)
        {
            Notifications.Add(new { Type = "session.started", SessionId = sessionId, TopicCode = topicCode, ScenarioCode = scenarioCode });
            return Task.CompletedTask;
        }

        public Task NotifyTranscriptPartialAsync(Guid sessionId, Guid? correlationId, int sequenceNumber, string role, string content, CancellationToken ct)
        {
            Notifications.Add(new { Type = "transcript.partial", SessionId = sessionId, SequenceNumber = sequenceNumber, Role = role, Content = content });
            return Task.CompletedTask;
        }

        public Task NotifyTranscriptFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, int sequenceNumber, string role, string content, DateTime createdAtUtc, CancellationToken ct)
        {
            Notifications.Add(new { Type = "transcript.final", SessionId = sessionId, MessageId = messageId, SequenceNumber = sequenceNumber, Role = role, Content = content, CreatedAtUtc = createdAtUtc });
            return Task.CompletedTask;
        }

        public Task NotifyAiResponsePartialAsync(Guid sessionId, Guid? correlationId, string content, CancellationToken ct)
        {
            Notifications.Add(new { Type = "ai.response.partial", SessionId = sessionId, Content = content });
            return Task.CompletedTask;
        }

        public Task NotifyAiResponseFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, string content, int sequenceNumber, CancellationToken ct)
        {
            Notifications.Add(new { Type = "ai.response.final", SessionId = sessionId, MessageId = messageId, Content = content, SequenceNumber = sequenceNumber });
            return Task.CompletedTask;
        }

        public Task NotifyCorrectionAvailableAsync(Guid sessionId, Guid? correlationId, Guid messageId, string originalText, string correctedText, string explanation, CancellationToken ct)
        {
            Notifications.Add(new { Type = "correction.available", SessionId = sessionId, MessageId = messageId, OriginalText = originalText, CorrectedText = correctedText, Explanation = explanation });
            return Task.CompletedTask;
        }

        public Task NotifyFeedbackReadyAsync(Guid sessionId, Guid? correlationId, string overallScore, string detailedFeedback, CancellationToken ct)
        {
            Notifications.Add(new { Type = "feedback.ready", SessionId = sessionId, OverallScore = overallScore, DetailedFeedback = detailedFeedback });
            return Task.CompletedTask;
        }

        public Task NotifySessionEndedAsync(Guid sessionId, Guid? correlationId, string reason, int durationSeconds, CancellationToken ct)
        {
            Notifications.Add(new { Type = "session.ended", SessionId = sessionId, Reason = reason, DurationSeconds = durationSeconds });
            return Task.CompletedTask;
        }

        public Task NotifyModelFallbackUsedAsync(Guid sessionId, Guid? correlationId, string primaryModel, string fallbackModel, string reason, CancellationToken ct)
        {
            Notifications.Add(new { Type = "model.fallback.used", SessionId = sessionId, PrimaryModel = primaryModel, FallbackModel = fallbackModel, Reason = reason });
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task JoinSession_Should_Succeed_For_Owner_And_RegisterConnection()
    {
        await using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var practice = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var registry = scope.ServiceProvider.GetRequiredService<IRealtimeConnectionRegistry>();

        var userId = Guid.NewGuid();
        var startResult = await practice.StartSessionAsync(userId, new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        startResult.Status.Should().Be(StartSessionStatus.Success);
        var sessionId = startResult.SessionId!.Value;

        // Join as owner
        var connectionId = "conn-owner-123";
        var joinResult = await sender.Send(new JoinPracticeSessionCommand(userId, sessionId, connectionId), CancellationToken.None);
        joinResult.IsSuccess.Should().BeTrue();

        // Check registry
        var conn = await registry.GetConnectionAsync(connectionId, CancellationToken.None);
        conn.Should().NotBeNull();
        conn!.UserId.Should().Be(userId);
        conn.SessionId.Should().Be(sessionId);

        // Check notification
        factory.Notifier.Notifications.Should().ContainSingle();
        var notification = factory.Notifier.Notifications[0];
        string type = ((dynamic)notification).Type;
        Guid sid = ((dynamic)notification).SessionId;
        type.Should().Be("session.started");
        sid.Should().Be(sessionId);
    }

    [Fact]
    public async Task JoinSession_Should_Reject_CrossUserAccess()
    {
        await using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var practice = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var ownerId = Guid.NewGuid();
        var startResult = await practice.StartSessionAsync(ownerId, new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        startResult.Status.Should().Be(StartSessionStatus.Success);
        var sessionId = startResult.SessionId!.Value;

        // Join as hacker
        var hackerId = Guid.NewGuid();
        var joinResult = await sender.Send(new JoinPracticeSessionCommand(hackerId, sessionId, "conn-hacker-123"), CancellationToken.None);
        joinResult.IsSuccess.Should().BeFalse();
        joinResult.Error.Should().NotBeNull();
        joinResult.Error!.Code.Should().Be("realtime.session.access_denied");
    }

    [Fact]
    public async Task LeaveSession_Should_UnregisterConnection()
    {
        await using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var practice = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var registry = scope.ServiceProvider.GetRequiredService<IRealtimeConnectionRegistry>();

        var userId = Guid.NewGuid();
        var startResult = await practice.StartSessionAsync(userId, new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var sessionId = startResult.SessionId!.Value;

        var connectionId = "conn-owner-123";
        await sender.Send(new JoinPracticeSessionCommand(userId, sessionId, connectionId), CancellationToken.None);

        // Leave session
        var leaveResult = await sender.Send(new LeavePracticeSessionCommand(userId, sessionId, connectionId), CancellationToken.None);
        leaveResult.IsSuccess.Should().BeTrue();

        // Check registry
        var conn = await registry.GetConnectionAsync(connectionId, CancellationToken.None);
        conn.Should().BeNull();
    }

    [Fact]
    public async Task Heartbeat_Should_UpdateLastSeenTimestamp()
    {
        await using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var practice = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var registry = scope.ServiceProvider.GetRequiredService<IRealtimeConnectionRegistry>();

        var userId = Guid.NewGuid();
        var startResult = await practice.StartSessionAsync(userId, new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var sessionId = startResult.SessionId!.Value;

        var connectionId = "conn-owner-123";
        await sender.Send(new JoinPracticeSessionCommand(userId, sessionId, connectionId), CancellationToken.None);

        var connBefore = await registry.GetConnectionAsync(connectionId, CancellationToken.None);
        var firstHeartbeat = connBefore!.LastHeartbeatAtUtc;

        // Perform heartbeat
        var heartbeatResult = await sender.Send(new RegisterHeartbeatCommand(connectionId), CancellationToken.None);
        heartbeatResult.IsSuccess.Should().BeTrue();

        var connAfter = await registry.GetConnectionAsync(connectionId, CancellationToken.None);
        connAfter!.LastHeartbeatAtUtc.Should().BeOnOrAfter(firstHeartbeat);
    }

    [Fact]
    public async Task PublishTranscript_Should_NotifyClients()
    {
        await using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var practice = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var userId = Guid.NewGuid();
        var startResult = await practice.StartSessionAsync(userId, new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15), CancellationToken.None);
        var sessionId = startResult.SessionId!.Value;

        // Join session
        var joinResult = await sender.Send(new JoinPracticeSessionCommand(userId, sessionId, "conn-123"), CancellationToken.None);
        joinResult.IsSuccess.Should().BeTrue();

        // Publish partial
        var partialResult = await sender.Send(new PublishUserTranscriptPartialCommand(userId, sessionId, 1, "Hello part"), CancellationToken.None);
        partialResult.IsSuccess.Should().BeTrue();

        // Publish final
        var finalResult = await sender.Send(new PublishUserTranscriptFinalCommand(userId, sessionId, 1, "Hello final"), CancellationToken.None);
        finalResult.IsSuccess.Should().BeTrue();

        // Verify notifications
        factory.Notifier.Notifications.Should().HaveCount(3); // 1. session.started, 2. transcript.partial, 3. transcript.final
        
        var partialNotify = factory.Notifier.Notifications[1];
        ((string)((dynamic)partialNotify).Type).Should().Be("transcript.partial");
        ((string)((dynamic)partialNotify).Content).Should().Be("Hello part");

        var finalNotify = factory.Notifier.Notifications[2];
        ((string)((dynamic)finalNotify).Type).Should().Be("transcript.final");
        ((string)((dynamic)finalNotify).Content).Should().Be("Hello final");
    }

    [Fact]
    public void GroupName_Should_Be_Safe_And_Deterministic()
    {
        var sessionId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var groupName = PracticeHub.GetGroupName(sessionId);
        groupName.Should().Be("practice.session.11111111222233334444555555555555");
    }

    [Fact]
    public void RealtimeModuleRegistration_Should_MapHubRoute()
    {
        using var factory = new RealtimeTestFactory();
        using var scope = factory.Services.CreateScope();
        
        var endpointSources = scope.ServiceProvider.GetServices<EndpointDataSource>();
        var hasHubRoute = endpointSources
            .SelectMany(s => s.Endpoints)
            .Any(e => e.DisplayName?.Contains("hubs/practice") == true || (e is RouteEndpoint re && re.RoutePattern.RawText == "/hubs/practice"));

        hasHubRoute.Should().BeTrue("the SignalR hub /hubs/practice must be mapped");
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

    private static async Task SeedAiGatewayAsync(IServiceScope scope, Guid scenarioId)
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var providerId = (await sender.Send(new CreateProviderCommand("Mock", "mock", true, null), CancellationToken.None)).Value;
        var modelId = (await sender.Send(new CreateModelCommand(
            providerId,
            "Mock Model",
            "mock-model",
            ["content-generation"],
            true,
            null), CancellationToken.None)).Value;
        await sender.Send(new CreateProviderKeyCommand(providerId, "primary", "sk-mock-123456", 0, true, null), CancellationToken.None);
        await sender.Send(new CreateRoutingRuleCommand("Practice Rule", ModeCode, TopicCode, scenarioId.ToString("N"), modelId, null, true, null), CancellationToken.None);
    }
}
