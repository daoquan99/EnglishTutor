using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Application.Commands.RecordAuditLog;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Repositories;
using FluentAssertions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EnglishTutor.Audit.UnitTests;

public class RecordAuditLogCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_For_Valid_Command()
    {
        var repository = new FakeAuditLogRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordAuditLogCommandHandler(repository, unitOfWork);

        var command = NewValidCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Persist_Exactly_One_AuditLog()
    {
        var repository = new FakeAuditLogRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordAuditLogCommandHandler(repository, unitOfWork);

        var command = NewValidCommand();

        await handler.Handle(command, CancellationToken.None);

        repository.Added.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Populate_AuditLog_With_Command_Fields()
    {
        var repository = new FakeAuditLogRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordAuditLogCommandHandler(repository, unitOfWork);

        var userId = Guid.NewGuid();
        var action = "create_api_key";
        var entityType = "ApiKey";
        var entityId = "key_1";
        var detailJson = "{\"scopes\":[\"read\"]}";
        var ipHash = "ip-hash";
        var uaHash = "ua-hash";
        var correlationId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var command = new RecordAuditLogCommand(
            UserId: userId,
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            DetailJson: detailJson,
            IpAddressHash: ipHash,
            UserAgentHash: uaHash,
            CorrelationId: correlationId,
            CreatedAtUtc: createdAt);

        await handler.Handle(command, CancellationToken.None);

        var stored = repository.Added.Single();
        stored.UserId.Should().Be(userId);
        stored.Action.Should().Be(action);
        stored.EntityType.Should().Be(entityType);
        stored.EntityId.Should().Be(entityId);
        stored.DetailJson.Should().Be(detailJson);
        stored.IpAddressHash.Should().Be(ipHash);
        stored.UserAgentHash.Should().Be(uaHash);
        stored.CorrelationId.Should().Be(correlationId);
        stored.CreatedAtUtc.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_Should_Call_UnitOfWork_SaveChangesAsync()
    {
        var repository = new FakeAuditLogRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordAuditLogCommandHandler(repository, unitOfWork);

        await handler.Handle(NewValidCommand(), CancellationToken.None);

        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Validator_Should_Pass_For_Valid_Command()
    {
        var validator = new RecordAuditLogCommandValidator();
        var command = NewValidCommand();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "EntityType", "EntityId", "{}", "Action is required.")]
    [InlineData("Action", "", "EntityId", "{}", "EntityType is required.")]
    [InlineData("Action", "EntityType", "", "{}", "EntityId is required.")]
    [InlineData("Action", "EntityType", "EntityId", "", "DetailJson is required.")]
    [InlineData("Action", "EntityType", "EntityId", "invalid_json", "DetailJson must be valid JSON.")]
    [InlineData("Action", "EntityType", "EntityId", "{\"password\":\"123\"}", "DetailJson must not contain sensitive markers.")]
    [InlineData("Action", "EntityType", "EntityId", "{\"secret\":\"abc\"}", "DetailJson must not contain sensitive markers.")]
    [InlineData("Action", "EntityType", "EntityId", "{\"token\":\"xyz\"}", "DetailJson must not contain sensitive markers.")]
    public async Task Validator_Should_Fail_For_Invalid_Inputs(
        string action,
        string entityType,
        string entityId,
        string detailJson,
        string expectedErrorMessage)
    {
        var validator = new RecordAuditLogCommandValidator();
        var command = new RecordAuditLogCommand(
            UserId: Guid.NewGuid(),
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            DetailJson: detailJson,
            IpAddressHash: null,
            UserAgentHash: null,
            CorrelationId: null,
            CreatedAtUtc: DateTime.UtcNow);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should().ContainMatch($"*{expectedErrorMessage}*");
    }

    private static RecordAuditLogCommand NewValidCommand()
    {
        return new RecordAuditLogCommand(
            UserId: Guid.NewGuid(),
            Action: "api_key_created",
            EntityType: "ApiKey",
            EntityId: "key_999",
            DetailJson: "{\"some\":\"details\"}",
            IpAddressHash: "ip_hash",
            UserAgentHash: "ua_hash",
            CorrelationId: Guid.NewGuid(),
            CreatedAtUtc: DateTime.UtcNow);
    }

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public List<AuditLog> Added { get; } = new();

        public Task AddAsync(AuditLog auditLog, CancellationToken ct)
        {
            Added.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(Added.FirstOrDefault(a => a.Id == id));
        }
    }

    private sealed class FakeAuditUnitOfWork : IAuditUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }
}
