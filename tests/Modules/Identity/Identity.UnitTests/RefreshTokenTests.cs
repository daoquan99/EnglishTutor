using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
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

    // Regression test for the Task 22A Correction 1 SessionId
    // removal: RefreshToken must NOT carry a persisted SessionId
    // property. The SessionId is supplied as runtime context to
    // MarkReuseDetected, not stored on the entity. This guards
    // against an accidental re-add of the property (which would
    // re-introduce the Identity schema change that was rolled back
    // in Correction 1 to keep Identity free of Audit-payload
    // columns).
    [Fact]
    public void RefreshToken_Should_Not_Have_Persisted_SessionId_Property()
    {
        var props = typeof(RefreshToken)
            .GetProperties(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        props.Should().NotContain("SessionId",
            "RefreshToken must NOT carry a persisted SessionId property. " +
            "SessionId is supplied as runtime context only via " +
            "MarkReuseDetected(nowUtc, reason, sessionId).");
    }

    // Regression test: the Issue factory must NOT take a sessionId
    // parameter. The factory was simplified in Task 22A Correction 1
    // to drop sessionId; this test pins the signature so a future
    // refactor that re-adds the parameter is caught.
    [Fact]
    public void RefreshToken_Issue_Should_Not_Take_SessionId_Parameter()
    {
        var issueMethods = typeof(RefreshToken)
            .GetMethods(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == "Issue");

        issueMethods.Should().ContainSingle(
            "RefreshToken.Issue must have exactly one overload and no sessionId parameter");

        var parameters = issueMethods.Single().GetParameters()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        parameters.Should().NotContain("sessionId");
    }

    // Regression test: MarkReuseDetected must accept the sessionId
    // as runtime context (parameter) and use it for the raised event
    // only — it must NOT persist it on the entity.
    [Fact]
    public void MarkReuseDetected_Should_Take_SessionId_As_Runtime_Context_Only()
    {
        var sessionId = Guid.NewGuid();
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.MarkUsed(Guid.NewGuid(), DateTime.UtcNow);

        var method = typeof(RefreshToken).GetMethod("MarkReuseDetected");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        parameters.Should().Contain("sessionId",
            "MarkReuseDetected must accept sessionId as a runtime parameter.");

        // Invoke and assert: SessionId ends up in the raised event, NOT
        // on the entity.
        token.MarkReuseDetected(DateTime.UtcNow, "refresh_token_reuse", sessionId);

        var reuseEvent = token.DomainEvents
            .OfType<EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.RefreshTokenReuseDetectedDomainEvent>()
            .SingleOrDefault();
        reuseEvent.Should().NotBeNull();
        reuseEvent!.SessionId.Should().Be(sessionId);

        // The entity itself must NOT have a SessionId property that
        // the caller can read back from token.SessionId.
        var sessionIdProp = typeof(RefreshToken).GetProperty("SessionId");
        sessionIdProp.Should().BeNull(
            "RefreshToken.SessionId must not be a persisted property; " +
            "it must only exist on the raised RefreshTokenReuseDetectedDomainEvent.");
    }
}
