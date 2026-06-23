using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using FluentAssertions;
using System;
using System.Text.Json;
using Xunit;

namespace EnglishTutor.Audit.UnitTests;

public class AuditLogTests
{
    [Fact]
    public void Create_Should_Initialize_All_Properties_And_StampCreated()
    {
        var userId = Guid.NewGuid();
        var action = "api_key_created";
        var entityType = "ApiKey";
        var entityId = "key_123456";
        var detailJson = "{\"key_name\":\"test_key\",\"scopes\":[\"read\"]}";
        var ipHash = "ip_hash_xyz";
        var uaHash = "ua_hash_xyz";
        var correlationId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var log = AuditLog.Create(
            userId: userId,
            action: action,
            entityType: entityType,
            entityId: entityId,
            detailJson: detailJson,
            ipAddressHash: ipHash,
            userAgentHash: uaHash,
            correlationId: correlationId,
            createdAtUtc: createdAt);

        log.Id.Should().NotBe(Guid.Empty);
        log.UserId.Should().Be(userId);
        log.Action.Should().Be(action);
        log.EntityType.Should().Be(entityType);
        log.EntityId.Should().Be(entityId);
        log.DetailJson.Should().Be(detailJson);
        log.IpAddressHash.Should().Be(ipHash);
        log.UserAgentHash.Should().Be(uaHash);
        log.CorrelationId.Should().Be(correlationId);
        log.CreatedAtUtc.Should().Be(createdAt);
    }

    [Theory]
    [InlineData("", "EntityType", "EntityId", "{}", "Action is required.")]
    [InlineData("Action", "", "EntityId", "{}", "EntityType is required.")]
    [InlineData("Action", "EntityType", "", "{}", "EntityId is required.")]
    [InlineData("Action", "EntityType", "EntityId", "", "DetailJson is required.")]
    [InlineData("Action", "EntityType", "EntityId", "invalid_json", "DetailJson must be valid JSON.")]
    public void Create_Should_Throw_ArgumentException_On_Validation_Failures(
        string action,
        string entityType,
        string entityId,
        string detailJson,
        string expectedErrorSnippet)
    {
        Action act = () => AuditLog.Create(
            userId: Guid.NewGuid(),
            action: action,
            entityType: entityType,
            entityId: entityId,
            detailJson: detailJson,
            ipAddressHash: null,
            userAgentHash: null,
            correlationId: null,
            createdAtUtc: DateTime.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*{expectedErrorSnippet}*");
    }

    [Fact]
    public void Create_Should_Throw_ArgumentException_When_DetailJson_Contains_Secrets()
    {
        var detailJsonWithPassword = "{\"username\":\"admin\",\"password\":\"supersecret\"}";

        Action act = () => AuditLog.Create(
            userId: Guid.NewGuid(),
            action: "login",
            entityType: "User",
            entityId: "123",
            detailJson: detailJsonWithPassword,
            ipAddressHash: null,
            userAgentHash: null,
            correlationId: null,
            createdAtUtc: DateTime.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*DetailJson contains sensitive markers.*");
    }

    [Fact]
    public void Create_Should_Throw_ArgumentException_On_Default_CreatedAtUtc()
    {
        Action act = () => AuditLog.Create(
            userId: Guid.NewGuid(),
            action: "test",
            entityType: "test",
            entityId: "test",
            detailJson: "{}",
            ipAddressHash: null,
            userAgentHash: null,
            correlationId: null,
            createdAtUtc: default);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*CreatedAtUtc must be a valid UTC timestamp.*");
    }
}
