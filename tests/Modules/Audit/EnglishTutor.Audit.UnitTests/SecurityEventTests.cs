using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using FluentAssertions;

namespace EnglishTutor.Audit.UnitTests;

public class SecurityEventTests
{
    [Fact]
    public void Create_Should_Initialize_All_Properties()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var occurred = DateTime.UtcNow;

        var e = SecurityEvent.Create(
            categoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            sourceModule: AuditCategoryCodes.SourceModuleIdentity,
            sourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            userId: userId,
            sessionId: sessionId,
            refreshTokenFamilyId: familyId,
            refreshTokenId: tokenId,
            reasonCode: "refresh_token_reuse",
            correlationId: correlationId,
            causationId: causationId,
            ipAddressHash: "abc123",
            userAgentHash: "def456",
            occurredAtUtc: occurred);

        e.Id.Should().NotBe(Guid.Empty);
        e.CategoryCode.Should().Be("identity.refresh_token_reuse_detected");
        e.SourceModule.Should().Be("identity");
        e.SourceEventType.Should().Be(AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected);
        e.UserId.Should().Be(userId);
        e.SessionId.Should().Be(sessionId);
        e.RefreshTokenFamilyId.Should().Be(familyId);
        e.RefreshTokenId.Should().Be(tokenId);
        e.ReasonCode.Should().Be("refresh_token_reuse");
        e.CorrelationId.Should().Be(correlationId);
        e.CausationId.Should().Be(causationId);
        e.IpAddressHash.Should().Be("abc123");
        e.UserAgentHash.Should().Be("def456");
        e.OccurredAtUtc.Should().Be(occurred);
    }

    [Theory]
    [InlineData(null, "identity", "X")]
    [InlineData("cat", null, "X")]
    [InlineData("cat", "identity", null)]
    public void Create_Should_Reject_Missing_Required_Fields(string? category, string? module, string? type)
    {
        Action act = () => SecurityEvent.Create(
            categoryCode: category!,
            sourceModule: module!,
            sourceEventType: type!,
            userId: null,
            sessionId: null,
            refreshTokenFamilyId: null,
            refreshTokenId: null,
            reasonCode: null,
            correlationId: null,
            causationId: null,
            ipAddressHash: null,
            userAgentHash: null,
            occurredAtUtc: DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
