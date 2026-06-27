using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Admin;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Contracts.Events;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace EnglishTutor.IntegrationTests.Practice;

[Collection("EnglishTutorIntegrationTests")]
public class PracticeDurabilityTests
{
    private const string ModeCode = "roleplay";
    private const string TopicCode = "job-interview";

    private sealed class FailingPracticePublisherFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
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
                var desc = services.FirstOrDefault(d => d.ServiceType == typeof(IPracticeIntegrationEventPublisher));
                if (desc != null)
                {
                    services.Remove(desc);
                }

                var mock = Substitute.For<IPracticeIntegrationEventPublisher>();
                mock.StageAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>())
                    .Returns(x => throw new InvalidOperationException("Forced Practice publisher outbox failure"));

                services.AddScoped<IPracticeIntegrationEventPublisher>(_ => mock);
            });
        }
    }

    private sealed class HappyPracticeFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
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
        }
    }

    [Fact]
    public async Task StartSession_WhenPublisherOutboxFails_ShouldRollbackAndNotCreateSession()
    {
        // Arrange
        await using var factory = new FailingPracticePublisherFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        // Act
        var act = async () => await module.StartSessionAsync(
            userId: userId,
            request: new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15),
            ct: CancellationToken.None);

        // Assert: staging failure should propagate and rollback transaction
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Forced Practice publisher outbox failure");

        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var sessions = await db.Sessions.Where(s => s.UserId == userId).ToListAsync();
        sessions.Should().BeEmpty("session creation must be rolled back on outbox staging failure");
    }

    [Fact]
    public async Task StartSession_HappyPath_ShouldStageStartedEventInPracticeSchemaOnly()
    {
        // Arrange
        await using var factory = new HappyPracticeFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);
        await SeedAiGatewayAsync(scope, scenarioId);

        var module = scope.ServiceProvider.GetRequiredService<IPracticeModule>();
        var userId = Guid.NewGuid();

        // Act
        var result = await module.StartSessionAsync(
            userId: userId,
            request: new StartSessionRequest(scenarioId, Guid.NewGuid().ToString("N"), 15),
            ct: CancellationToken.None);

        // Assert
        result.Status.Should().Be(StartSessionStatus.Success);

        var db = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var outboxMessages = await db.Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>().ToListAsync();
        
        // Find started event
        var startedEvent = outboxMessages.FirstOrDefault(m => m.Body != null && m.Body.Contains("PracticeSessionStartedIntegrationEventV1"));
        startedEvent.Should().NotBeNull("happy path session start must stage started integration event");
    }

    private static async Task MigrateAllAsync(IServiceScope scope)
    {
        await scope.ServiceProvider.GetRequiredService<PracticeDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<EnglishTutor.AiGateway.Infrastructure.Persistence.AiGatewayDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<EnglishTutor.Quota.Infrastructure.Persistence.QuotaDbContext>().Database.MigrateAsync();
    }

    private static async Task<Guid> SeedScenarioReadModelAsync(IServiceScope scope)
    {
        var repo = scope.ServiceProvider.GetRequiredService<IPracticeScenarioReadModelRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IPracticeUnitOfWork>();
        var scenarioId = Guid.NewGuid();
        var snapshot = new PracticeScenarioReadModel(
            scenarioId: scenarioId,
            topicId: Guid.NewGuid(),
            topicCode: TopicCode,
            topicTitle: "Job Interview",
            modeDefinitionId: Guid.NewGuid(),
            modeCode: ModeCode,
            title: "Salary Negotiation",
            learnerFacingInstructions: "Help the learner negotiate a salary.");
        await repo.AddAsync(snapshot, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);
        return scenarioId;
    }

    private static async Task SeedAiGatewayAsync(IServiceScope scope, Guid scenarioId)
    {
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<MediatR.ISender>());
        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(providerId, "Mock Model", "mock-model", [ModeCode], true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "primary", "sk-mock-123456", 0, true), null, CancellationToken.None);

        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput(
            "Practice Rule", ModeCode, TopicCode, scenarioId.ToString("N"), modelId, null, true), null, CancellationToken.None);
    }

    private sealed class TestAdminFacade
    {
        private readonly MediatR.ISender _sender;
        public TestAdminFacade(MediatR.ISender sender) => _sender = sender;

        public async Task<BuildingBlocks.Domain.Results.Result<Guid>> CreateProviderAsync(CreateProviderInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider.CreateProviderCommand(input.Name, input.Code, input.IsActive, actor), ct);

        public async Task<BuildingBlocks.Domain.Results.Result<Guid>> CreateProviderKeyAsync(CreateProviderKeyInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey.CreateProviderKeyCommand(input.ProviderId, input.Name, input.Secret, input.Priority, input.IsActive, actor), ct);

        public async Task<BuildingBlocks.Domain.Results.Result<Guid>> CreateModelAsync(CreateModelInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel.CreateModelCommand(input.ProviderId, input.Name, input.Code, input.Capabilities, input.IsActive, actor), ct);

        public async Task<BuildingBlocks.Domain.Results.Result<Guid>> CreateRoutingRuleAsync(CreateRoutingRuleInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule.CreateRoutingRuleCommand(input.Name, input.ActivityType, input.TopicCode, input.ScenarioCode, input.PrimaryModelId, input.FallbackModelId, input.IsActive, actor), ct);
    }
}
