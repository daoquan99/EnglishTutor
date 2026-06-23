using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.Repositories;
using EnglishTutor.BuildingBlocks.Domain.Results;
using FluentAssertions;

namespace EnglishTutor.Audit.UnitTests;

// Unit tests for RecordSecurityEventCommandHandler.
//
// The handler is the write side of the Audit pipeline. Identity (and any
// future module) calls Audit.Contracts.ISecurityEventRecorder, which
// Audit.Infrastructure.SecurityEventRecorder translates into this
// command and dispatches via MediatR. These tests pin:
//   - command → handler returns Result.Success
//   - exactly one SecurityEvent is added to the repository
//   - the added aggregate carries the exact payload fields from the
//     command (no transformation, no defaults substituted silently)
//   - the SecurityEvent does NOT carry a raw refresh token, a
//     refresh-token hash, or any other sensitive field (rule 41 § 2)
public class RecordSecurityEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_For_RefreshToken_Reuse_Command()
    {
        var repository = new FakeSecurityEventRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordSecurityEventCommandHandler(repository, unitOfWork);

        var command = NewRefreshTokenReuseCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Persist_Exactly_One_SecurityEvent()
    {
        var repository = new FakeSecurityEventRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordSecurityEventCommandHandler(repository, unitOfWork);

        var command = NewRefreshTokenReuseCommand();

        await handler.Handle(command, CancellationToken.None);

        repository.Added.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Populate_SecurityEvent_With_Command_Fields()
    {
        var repository = new FakeSecurityEventRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordSecurityEventCommandHandler(repository, unitOfWork);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var command = new RecordSecurityEventCommand(
            CategoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            UserId: userId,
            SessionId: sessionId,
            RefreshTokenFamilyId: familyId,
            RefreshTokenId: tokenId,
            ReasonCode: "refresh_token_reuse",
            CorrelationId: correlationId,
            CausationId: causationId,
            IpAddressHash: "ip-hash-abc",
            UserAgentHash: "ua-hash-def",
            OccurredAtUtc: occurredAt);

        await handler.Handle(command, CancellationToken.None);

        var stored = repository.Added.Single();
        stored.CategoryCode.Should().Be(AuditCategoryCodes.IdentityRefreshTokenReuseDetected);
        stored.SourceModule.Should().Be(AuditCategoryCodes.SourceModuleIdentity);
        stored.SourceEventType.Should().Be(AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected);
        stored.UserId.Should().Be(userId);
        stored.SessionId.Should().Be(sessionId);
        stored.RefreshTokenFamilyId.Should().Be(familyId);
        stored.RefreshTokenId.Should().Be(tokenId);
        stored.ReasonCode.Should().Be("refresh_token_reuse");
        stored.CorrelationId.Should().Be(correlationId);
        stored.CausationId.Should().Be(causationId);
        stored.IpAddressHash.Should().Be("ip-hash-abc");
        stored.UserAgentHash.Should().Be("ua-hash-def");
        stored.OccurredAtUtc.Should().Be(occurredAt);
    }

    [Fact]
    public async Task Handle_Should_Not_Persist_Raw_RefreshToken_Or_Token_Hash()
    {
        // Rule 41 § 2: the audit pipeline must never persist a raw
        // refresh token, a refresh-token hash, or any plaintext token
        // value. The handler's only persistence surface is the
        // SecurityEvent aggregate; this test enumerates every property
        // of the stored aggregate and asserts none of them contain a
        // "refresh token" / "token hash" / "raw token" payload.
        var repository = new FakeSecurityEventRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordSecurityEventCommandHandler(repository, unitOfWork);

        // Construct a command that tries to smuggle in raw token /
        // hash strings in the only fields it has (ReasonCode,
        // IpAddressHash, UserAgentHash). These are the only "free
        // string" inputs the handler accepts.
        var command = new RecordSecurityEventCommand(
            CategoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            RefreshTokenFamilyId: Guid.NewGuid(),
            RefreshTokenId: Guid.NewGuid(),
            ReasonCode: "refresh_token_reuse",
            CorrelationId: null,
            CausationId: null,
            IpAddressHash: "abc123-ip-hash",       // already a hash, not a raw token
            UserAgentHash: "abc123-ua-hash",     // already a hash, not a raw token
            OccurredAtUtc: DateTime.UtcNow);

        await handler.Handle(command, CancellationToken.None);

        var stored = repository.Added.Single();

        // Only ids and reason codes are stored. No raw token, no token
        // hash, no plaintext token representation. This is a structural
        // assertion: the SecurityEvent aggregate has no string field
        // that could hold a token, and the command has no field that
        // could carry a raw token into the handler.
        var stringProperties = stored.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        stringProperties.Should().NotContain("RawRefreshToken");
        stringProperties.Should().NotContain("RefreshTokenHash");
        stringProperties.Should().NotContain("Token");
        stringProperties.Should().NotContain("TokenHash");
        stringProperties.Should().NotContain("RawToken");
        stringProperties.Should().NotContain("Password");

        // Confirm the public API of SecurityEvent exposes only the
        // documented fields (defensive against future regressions).
        stringProperties.Should().BeSubsetOf(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CategoryCode", "SourceModule", "SourceEventType", "ReasonCode",
            "IpAddressHash", "UserAgentHash"
        });
    }

    [Fact]
    public async Task Handle_Should_Call_UnitOfWork_SaveChangesAsync()
    {
        var repository = new FakeSecurityEventRepository();
        var unitOfWork = new FakeAuditUnitOfWork();
        var handler = new RecordSecurityEventCommandHandler(repository, unitOfWork);

        await handler.Handle(NewRefreshTokenReuseCommand(), CancellationToken.None);

        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    private static RecordSecurityEventCommand NewRefreshTokenReuseCommand()
    {
        return new RecordSecurityEventCommand(
            CategoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            RefreshTokenFamilyId: Guid.NewGuid(),
            RefreshTokenId: Guid.NewGuid(),
            ReasonCode: "refresh_token_reuse",
            CorrelationId: Guid.NewGuid(),
            CausationId: Guid.NewGuid(),
            IpAddressHash: "ip-hash-abc",
            UserAgentHash: "ua-hash-def",
            OccurredAtUtc: DateTime.UtcNow);
    }

    private sealed class FakeSecurityEventRepository : ISecurityEventRepository
    {
        public List<SecurityEvent> Added { get; } = new();

        public Task AddAsync(SecurityEvent securityEvent, CancellationToken ct)
        {
            Added.Add(securityEvent);
            return Task.CompletedTask;
        }

        public Task<SecurityEvent?> FindByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(Added.FirstOrDefault(s => s.Id == id));
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
