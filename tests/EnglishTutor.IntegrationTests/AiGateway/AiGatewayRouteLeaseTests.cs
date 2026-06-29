using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using EnglishTutor.AiGateway.Application.Admin;
using MediatR;
using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Queries.ListProviderKeys;
using EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.AiGateway;

// Service-level feature tests for AiGateway route-lease selection, key
// encryption/masking, and the provider adapter baseline. They run against the
// real DI graph + temporary Postgres database (no external provider calls).
[Collection("EnglishTutorIntegrationTests")]
public class AiGatewayRouteLeaseTests
{
    private sealed class AiGatewayTestFactory : IntegrationTestFactory { }

    private const string Activity = "roleplay";
    private const string Topic = "job-interview";
    private const string Scenario = "salary-negotiation";

    [Fact]
    public async Task ProviderKey_Should_Be_Encrypted_At_Rest_And_Masked()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);

        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;

        const string secret = "sk-supersecret-value-1234";
        var keyResult = await admin.CreateProviderKeyAsync(
            new CreateProviderKeyInput(providerId, "primary", secret, 0, true), null, CancellationToken.None);
        keyResult.IsSuccess.Should().BeTrue();

        var db = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        var stored = await db.ProviderKeys.AsNoTracking().FirstAsync(k => k.Id == keyResult.Value);

        stored.EncryptedKey.Should().NotContain(secret, "the secret must be encrypted at rest");
        stored.KeyMask.Should().Be("****1234");
        stored.KeyMask.Should().NotContain(secret);

        var listed = (await admin.ListProviderKeysAsync(providerId, CancellationToken.None)).Value!;
        listed.Should().ContainSingle();
        listed[0].KeyMask.Should().Be("****1234");

        // The protector round-trips so execution can use the real secret internally.
        var protector = scope.ServiceProvider.GetRequiredService<IAiKeyProtector>();
        protector.Decrypt(stored.EncryptedKey).Should().Be(secret);
    }

    [Fact]
    public async Task CreateRouteLease_Should_Pick_Active_Key_By_Priority()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(
            providerId,
            "Mock Model",
            "mock-model",
            ["content-generation"],
            true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "low", "sk-low", 10, true), null, CancellationToken.None);
        var highKey = (await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "high", "sk-high", 0, true), null, CancellationToken.None)).Value;
        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput("Rule", Activity, Topic, Scenario, modelId, null, true), null, CancellationToken.None);

        var lease = await module.CreateRouteLeaseAsync(NewLeaseRequest(), CancellationToken.None);

        lease.Status.Should().Be(CreateRouteLeaseStatus.Success);
        lease.LeaseId.Should().NotBeNull();
        lease.ModelCode.Should().Be("mock-model");
        lease.ProviderCode.Should().Be("mock");

        var db = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        var stored = await db.RouteLeases.AsNoTracking().FirstAsync(l => l.Id == lease.LeaseId);
        stored.ProviderKeyId.Should().Be(highKey, "the highest-priority active key must be selected");
    }

    [Fact]
    public async Task CreateRouteLease_Should_Skip_Cooldown_Keys()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var uow = scope.ServiceProvider.GetRequiredService<IAiGatewayUnitOfWork>();
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(providerId, "Mock Model", "mock-model", [], true), null, CancellationToken.None)).Value;
        var cooledKeyId = (await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "cooled", "sk-cooled", 0, true), null, CancellationToken.None)).Value;
        var freeKeyId = (await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "free", "sk-free", 10, true), null, CancellationToken.None)).Value;
        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput("Rule", Activity, Topic, Scenario, modelId, null, true), null, CancellationToken.None);

        // Put the higher-priority key on cooldown.
        var cooled = await uow.ProviderKeys.GetByIdAsync(cooledKeyId, CancellationToken.None);
        cooled!.SetCooldown(TimeSpan.FromMinutes(30));
        uow.ProviderKeys.Update(cooled);
        await uow.SaveChangesAsync(CancellationToken.None);

        var lease = await module.CreateRouteLeaseAsync(NewLeaseRequest(), CancellationToken.None);

        lease.Status.Should().Be(CreateRouteLeaseStatus.Success);
        var db = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        var stored = await db.RouteLeases.AsNoTracking().FirstAsync(l => l.Id == lease.LeaseId);
        stored.ProviderKeyId.Should().Be(freeKeyId, "a key on cooldown must be skipped");
    }

    [Fact]
    public async Task CreateRouteLease_Should_Use_Fallback_Model_When_Primary_Has_No_Key()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        // Primary provider has NO active key; fallback provider does.
        var primaryProvider = (await admin.CreateProviderAsync(new CreateProviderInput("Primary", "primary", true), null, CancellationToken.None)).Value;
        var primaryModel = (await admin.CreateModelAsync(new CreateModelInput(primaryProvider, "Primary Model", "primary-model", [], true), null, CancellationToken.None)).Value;

        var fallbackProvider = (await admin.CreateProviderAsync(new CreateProviderInput("Fallback", "mock", true), null, CancellationToken.None)).Value;
        var fallbackModel = (await admin.CreateModelAsync(new CreateModelInput(fallbackProvider, "Fallback Model", "fallback-model", [], true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(fallbackProvider, "fb", "sk-fb", 0, true), null, CancellationToken.None);

        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput("Rule", Activity, Topic, Scenario, primaryModel, fallbackModel, true), null, CancellationToken.None);

        var lease = await module.CreateRouteLeaseAsync(NewLeaseRequest(), CancellationToken.None);

        lease.Status.Should().Be(CreateRouteLeaseStatus.Success);
        lease.ModelCode.Should().Be("fallback-model", "the fallback model must be used when the primary has no usable key");
    }

    [Fact]
    public async Task CreateRouteLease_Should_Be_Idempotent_For_Same_Key()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(providerId, "Mock Model", "mock-model", [], true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "k", "sk-k", 0, true), null, CancellationToken.None);
        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput("Rule", Activity, Topic, Scenario, modelId, null, true), null, CancellationToken.None);

        var userId = Guid.NewGuid();
        var first = await module.CreateRouteLeaseAsync(NewLeaseRequest("idem-1", userId), CancellationToken.None);
        var second = await module.CreateRouteLeaseAsync(NewLeaseRequest("idem-1", userId), CancellationToken.None);

        first.Status.Should().Be(CreateRouteLeaseStatus.Success);
        second.Status.Should().Be(CreateRouteLeaseStatus.IdempotentRepeat);
        second.LeaseId.Should().Be(first.LeaseId);
    }

    [Fact]
    public async Task CreateRouteLease_Should_Return_NoMatch_When_No_Active_Rule()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        var lease = await module.CreateRouteLeaseAsync(NewLeaseRequest(), CancellationToken.None);

        lease.Status.Should().Be(CreateRouteLeaseStatus.RouteNoMatch);
        lease.LeaseId.Should().BeNull();
    }

    [Fact]
    public async Task Execute_Confirm_Release_Lifecycle_Should_Work_With_Mock_Adapter()
    {
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAsync(scope);
        var admin = new TestAdminFacade(scope.ServiceProvider.GetRequiredService<ISender>());
        var module = scope.ServiceProvider.GetRequiredService<IAiGatewayModule>();

        var providerId = (await admin.CreateProviderAsync(new CreateProviderInput("Mock", "mock", true), null, CancellationToken.None)).Value;
        var modelId = (await admin.CreateModelAsync(new CreateModelInput(providerId, "Mock Model", "mock-model", [], true), null, CancellationToken.None)).Value;
        await admin.CreateProviderKeyAsync(new CreateProviderKeyInput(providerId, "k", "sk-k", 0, true), null, CancellationToken.None);
        await admin.CreateRoutingRuleAsync(new CreateRoutingRuleInput("Rule", Activity, Topic, Scenario, modelId, null, true), null, CancellationToken.None);

        var lease = await module.CreateRouteLeaseAsync(NewLeaseRequest(), CancellationToken.None);
        lease.LeaseId.Should().NotBeNull();

        var exec = await module.ExecuteChatCompletionAsync(new ExecuteChatCompletionRequest
        {
            LeaseId = lease.LeaseId!.Value,
            SystemPrompt = "You are a helpful tutor.",
            UserPrompt = "Hello",
        }, CancellationToken.None);

        exec.Status.Should().Be(ExecuteChatCompletionStatus.Success);
        exec.ResponseText.Should().NotBeNullOrWhiteSpace();
        exec.ResponseText.Should().NotContain("sk-k", "the raw provider secret must never appear in output");
        exec.PromptTokens.Should().BeGreaterThan(0);

        var confirm = await module.ConfirmRouteUsageAsync(new ConfirmRouteUsageRequest
        {
            LeaseId = lease.LeaseId.Value,
            PromptTokens = exec.PromptTokens,
            CompletionTokens = exec.CompletionTokens,
        }, CancellationToken.None);
        confirm.Status.Should().Be(ConfirmRouteUsageStatus.Success);

        // A confirmed lease can no longer be released.
        var release = await module.ReleaseRouteLeaseAsync(new ReleaseRouteLeaseRequest { LeaseId = lease.LeaseId.Value }, CancellationToken.None);
        release.Status.Should().Be(ReleaseRouteLeaseStatus.InvalidLeaseState);
    }

    private static CreateRouteLeaseRequest NewLeaseRequest(string? idem = null, Guid? userId = null) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        ActivityType = Activity,
        TopicCode = Topic,
        ScenarioCode = Scenario,
        IdempotencyKey = idem ?? Guid.NewGuid().ToString("N"),
    };

    private static async Task MigrateAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        await db.Database.MigrateAsync();
    }

    private sealed class TestAdminFacade
    {
        private readonly ISender _sender;
        public TestAdminFacade(ISender sender) => _sender = sender;

        public async Task<Result<Guid>> CreateProviderAsync(CreateProviderInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateProviderCommand(input.Name, input.Code, input.IsActive, actor), ct);

        public async Task<Result<Guid>> CreateProviderKeyAsync(CreateProviderKeyInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateProviderKeyCommand(input.ProviderId, input.Name, input.Secret, input.Priority, input.IsActive, actor), ct);

        public async Task<Result<IReadOnlyList<ProviderKeyView>>> ListProviderKeysAsync(Guid providerId, CancellationToken ct) =>
            await _sender.Send(new ListProviderKeysQuery(providerId), ct);

        public async Task<Result<Guid>> CreateModelAsync(CreateModelInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateModelCommand(input.ProviderId, input.Name, input.Code, input.Capabilities, input.IsActive, actor), ct);

        public async Task<Result<Guid>> CreateRoutingRuleAsync(CreateRoutingRuleInput input, Guid? actor, CancellationToken ct) =>
            await _sender.Send(new CreateRoutingRuleCommand(input.Name, input.ActivityType, input.TopicCode, input.ScenarioCode, input.PrimaryModelId, input.FallbackModelId, input.IsActive, actor), ct);
    }
}
