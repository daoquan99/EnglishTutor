using EnglishTutor.Audit.Contracts;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using EnglishTutor.Identity.Infrastructure.Audit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnglishTutor.Identity.UnitTests;

public class RefreshTokenReuseAuditHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Forward_All_Ids_To_Recorder()
    {
        var recorder = new FakeSecurityEventRecorder();
        var accessor = new FakeContextAccessor(
            ipAddressHash: "ip123",
            userAgentHash: "ua456",
            correlationId: Guid.NewGuid());

        var handler = new RefreshTokenReuseAuditHandler(
            recorder, accessor, NullLogger<RefreshTokenReuseAuditHandler>.Instance);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var occurred = DateTime.UtcNow;

        var domainEvent = new RefreshTokenReuseDetectedDomainEvent(
            userId, sessionId, familyId, tokenId, Reason: "refresh_token_reuse");

        // OccurredAtUtc is auto-set to DateTime.UtcNow; the test does not
        // pin the exact value to avoid timing flakiness, but the handler
        // forwards the value as-is so the recorder sees the same instant.
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        recorder.Calls.Should().HaveCount(1);
        var recorded = recorder.Calls[0];
        recorded.UserId.Should().Be(userId);
        recorded.SessionId.Should().Be(sessionId);
        recorded.RefreshTokenFamilyId.Should().Be(familyId);
        recorded.RefreshTokenId.Should().Be(tokenId);
        recorded.ReasonCode.Should().Be("refresh_token_reuse");
        recorded.IpAddressHash.Should().Be("ip123");
        recorded.UserAgentHash.Should().Be("ua456");
        recorded.OccurredAtUtc.Should().Be(domainEvent.OccurredAtUtc);
    }

    [Fact]
    public async Task HandleAsync_Should_Pass_Null_For_Missing_Context()
    {
        var recorder = new FakeSecurityEventRecorder();
        var accessor = new FakeContextAccessor(null, null, null);

        var handler = new RefreshTokenReuseAuditHandler(
            recorder, accessor, NullLogger<RefreshTokenReuseAuditHandler>.Instance);

        var domainEvent = new RefreshTokenReuseDetectedDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Reason: "r");

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        recorder.Calls.Should().HaveCount(1);
        var recorded = recorder.Calls[0];
        recorded.IpAddressHash.Should().BeNull();
        recorded.UserAgentHash.Should().BeNull();
        recorded.CorrelationId.Should().BeNull();
    }
}

internal sealed class FakeSecurityEventRecorder : ISecurityEventRecorder
{
    public List<RecordRefreshTokenReuseRequest> Calls { get; } = new();
    public List<RecordSecurityEventRequest> SecurityEventCalls { get; } = new();

    public Task RecordRefreshTokenReuseAsync(
        RecordRefreshTokenReuseRequest request,
        CancellationToken cancellationToken = default)
    {
        Calls.Add(request);
        return Task.CompletedTask;
    }

    public Task RecordSecurityEventAsync(
        RecordSecurityEventRequest request,
        CancellationToken cancellationToken = default)
    {
        SecurityEventCalls.Add(request);
        return Task.CompletedTask;
    }
}

internal sealed class FakeContextAccessor : IRefreshTokenReuseContextAccessor
{
    public FakeContextAccessor(string? ipAddressHash, string? userAgentHash, Guid? correlationId)
    {
        IpAddressHash = ipAddressHash;
        UserAgentHash = userAgentHash;
        CorrelationId = correlationId;
    }

    public string? IpAddressHash { get; }
    public string? UserAgentHash { get; }
    public Guid? CorrelationId { get; }
}
