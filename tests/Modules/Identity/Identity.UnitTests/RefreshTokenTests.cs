using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

public class RefreshTokenTests
{
    [Fact]
    public void Issue_Should_Initialize_Token_And_Family_Id()
    {
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var hash = "abcdef1234567890";

        var token = RefreshToken.Issue(userId, familyId, hash, DateTime.UtcNow.AddDays(7), "127.0.0.1");

        token.Id.Should().NotBe(Guid.Empty);
        token.UserId.Should().Be(userId);
        token.FamilyId.Should().Be(familyId);
        token.TokenHash.Should().Be(hash);
        token.UsedAtUtc.Should().BeNull();
        token.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public void IsActive_Should_Be_True_For_Fresh_Token()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_Should_Be_False_For_Expired_Token()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddSeconds(-1), null);
        token.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsActive_Should_Be_False_After_Revoked()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.RevokeFamily(DateTime.UtcNow, null, Guid.NewGuid());
        token.IsActive().Should().BeFalse();
    }

    [Fact]
    public void MarkUsed_Should_Stamp_UsedAt_And_Link_To_Replacement()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        var replacementId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        token.MarkUsed(replacementId, now);

        token.UsedAtUtc.Should().Be(now);
        token.ReplacedByTokenId.Should().Be(replacementId);
    }

    [Fact]
    public void MarkUsed_Twice_Should_Throw()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.MarkUsed(Guid.NewGuid(), DateTime.UtcNow);
        var act = () => token.MarkUsed(Guid.NewGuid(), DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    // DetectReuse is preserved as a compatibility wrapper. It now
    // requires the caller to supply a session id (no longer read
    // from a deleted property).
    // DetectReuse is preserved as a compatibility wrapper on
    // UserSession, not on RefreshToken (the token does not know its
    // session id). This test exercises the new domain path: the
    // session's DetectRefreshTokenReuse method marks the token as
    // reuse-detected with the supplied session id and raises the
    // event with that session id.
    [Fact]
    public void MarkReuseDetected_Should_Raise_Event_With_Supplied_Session_Id()
    {
        var sessionId = Guid.NewGuid();
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.MarkUsed(Guid.NewGuid(), DateTime.UtcNow);

        token.MarkReuseDetected(DateTime.UtcNow, reason: "refresh_token_reuse", sessionId: sessionId);

        token.RevokedAtUtc.Should().NotBeNull();
        var reuseEvent = token.DomainEvents.SingleOrDefault(e =>
            e.GetType().Name == "RefreshTokenReuseDetectedDomainEvent");
        reuseEvent.Should().NotBeNull();
        reuseEvent!.GetType().GetProperty("SessionId")!.GetValue(reuseEvent)
            .Should().Be(sessionId);
    }
}
