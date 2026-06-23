using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

// Unit tests for UserSession, focused on the refresh-token reuse
// detection path added in Task 22A.
//
// Background: prior to Task 22A, RefreshToken.MarkReuseDetected raised
// the RefreshTokenReuseDetectedDomainEvent directly. Because the
// RefreshToken entity had a persisted SessionId column, the event
// could pull SessionId from the entity. After the Task 22A Correction
// 1 pass the SessionId was removed from RefreshToken (to keep the
// Identity schema free of Audit-payload columns); the event must
// therefore receive SessionId as runtime context from the aggregate
// root (UserSession.DetectRefreshTokenReuse) which still has it.
public class UserSessionTests
{
    [Fact]
    public void DetectRefreshTokenReuse_Should_Raise_Event_With_Correct_Payload()
    {
        // Arrange: a freshly-created UserSession with one consumed
        // refresh token. The token is consumed (UsedAtUtc set) so the
        // reuse-detection path is triggered.
        var userId = Guid.NewGuid();
        var device = DeviceInfo.Create(
            deviceId: "device-abc",
            deviceName: "Test Browser",
            userAgent: "Mozilla/5.0 Test",
            ipAddress: "127.0.0.1");
        var nowUtc = DateTime.UtcNow;

        var session = UserSession.Create(userId, device, nowUtc);
        var sessionId = session.Id;
        var familyId = session.Family!.Id;

        var token = RefreshToken.Issue(
            userId, familyId, "hash-abc", nowUtc.AddDays(7), createdByIp: "127.0.0.1");
        session.Family.AddToken(token);

        // Simulate a previous successful refresh: token is consumed.
        token.Consume(replacementTokenId: Guid.NewGuid(), nowUtc: nowUtc);

        // Act: detect reuse.
        session.DetectRefreshTokenReuse(token, nowUtc.AddSeconds(1), reason: "refresh_token_reuse");

        // Assert: the raised domain event carries the expected ids and
        // reason. The audit pipeline subscribes to this event and the
        // Audit row must carry these exact values.
        //
        // The event is raised on the RefreshToken entity (via
        // token.MarkReuseDetected -> RaiseDomainEvent) — RefreshToken
        // is an AggregateRoot too. AggregateRoot.DomainEvents returns
        // events raised on that specific aggregate, so we look at
        // token.DomainEvents, not session.DomainEvents.
        var reuseEvent = token.DomainEvents
            .OfType<RefreshTokenReuseDetectedDomainEvent>()
            .SingleOrDefault();

        reuseEvent.Should().NotBeNull("DetectRefreshTokenReuse must raise exactly one RefreshTokenReuseDetectedDomainEvent");
        reuseEvent!.UserId.Should().Be(userId);
        reuseEvent.SessionId.Should().Be(sessionId);
        reuseEvent.RefreshTokenFamilyId.Should().Be(familyId);
        reuseEvent.RefreshTokenId.Should().Be(token.Id);
        reuseEvent.Reason.Should().Be("refresh_token_reuse");
    }

    [Fact]
    public void DetectRefreshTokenReuse_Should_Revoke_Session_And_Family()
    {
        // Arrange.
        var userId = Guid.NewGuid();
        var device = DeviceInfo.Create(
            deviceId: "device-abc",
            deviceName: null,
            userAgent: "Mozilla/5.0 Test",
            ipAddress: null);
        var nowUtc = DateTime.UtcNow;

        var session = UserSession.Create(userId, device, nowUtc);
        var familyId = session.Family!.Id;

        var token = RefreshToken.Issue(
            userId, familyId, "hash-abc", nowUtc.AddDays(7), createdByIp: null);
        session.Family.AddToken(token);
        token.Consume(replacementTokenId: Guid.NewGuid(), nowUtc: nowUtc);

        // Act.
        session.DetectRefreshTokenReuse(token, nowUtc.AddSeconds(1), reason: "refresh_token_reuse");

        // Assert: the session is revoked.
        session.RevokedAtUtc.Should().NotBeNull();
        session.RevokedReason.Should().Be("refresh_token_reuse");

        // Assert: the family is revoked.
        session.Family!.RevokedAtUtc.Should().NotBeNull();
        session.Family!.RevokedReason.Should().Be("refresh_token_reuse");

        // Assert: the token itself is revoked. RevokeFamily is the
        // leaf-level revoke: sets RevokedAtUtc.
        token.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void DetectRefreshTokenReuse_Should_Throw_When_Token_Belongs_To_Different_Family()
    {
        // Arrange: a session with its own family, and a token that
        // belongs to a DIFFERENT family. Passing the wrong token is a
        // programming error and must throw.
        var session = UserSession.Create(
            userId: Guid.NewGuid(),
            device: DeviceInfo.Create("d1", null, "ua-1", null),
            nowUtc: DateTime.UtcNow);

        var foreignToken = RefreshToken.Issue(
            userId: session.UserId,
            familyId: Guid.NewGuid(),  // <-- a different family
            tokenHash: "hash-foreign",
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            createdByIp: null);

        var act = () => session.DetectRefreshTokenReuse(
            foreignToken, DateTime.UtcNow, "refresh_token_reuse");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not belong to this session's family*");
    }

    [Fact]
    public void DetectRefreshTokenReuse_Should_Throw_When_Reason_Is_Null_Or_Empty()
    {
        var session = UserSession.Create(
            userId: Guid.NewGuid(),
            device: DeviceInfo.Create("d1", null, "ua-1", null),
            nowUtc: DateTime.UtcNow);

        var token = RefreshToken.Issue(
            userId: session.UserId,
            familyId: session.Family!.Id,
            tokenHash: "hash-abc",
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            createdByIp: null);
        session.Family.AddToken(token);
        token.Consume(replacementTokenId: Guid.NewGuid(), nowUtc: DateTime.UtcNow);

        var actNull = () => session.DetectRefreshTokenReuse(token, DateTime.UtcNow, reason: null!);
        var actEmpty = () => session.DetectRefreshTokenReuse(token, DateTime.UtcNow, reason: "");

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DetectRefreshTokenReuse_Should_Raise_UserSessionRevoked_And_UserLoggedOut_Domain_Events()
    {
        // The Revoke() helper inside DetectRefreshTokenReuse raises two
        // additional events: UserSessionRevokedDomainEvent (for any
        // revocation cause) and UserLoggedOutDomainEvent (user-initiated
        // logout). This is the existing behavior carried over from
        // Task 22A Correction 1; the test pins it so future refactors
        // don't silently drop the events.
        var session = UserSession.Create(
            userId: Guid.NewGuid(),
            device: DeviceInfo.Create("d1", null, "ua-1", null),
            nowUtc: DateTime.UtcNow);

        var token = RefreshToken.Issue(
            userId: session.UserId,
            familyId: session.Family!.Id,
            tokenHash: "hash-abc",
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            createdByIp: null);
        session.Family.AddToken(token);
        token.Consume(replacementTokenId: Guid.NewGuid(), nowUtc: DateTime.UtcNow);

        session.DetectRefreshTokenReuse(token, DateTime.UtcNow, reason: "refresh_token_reuse");

        session.DomainEvents.OfType<UserSessionRevokedDomainEvent>().Should().ContainSingle();
        session.DomainEvents.OfType<UserLoggedOutDomainEvent>().Should().ContainSingle();
    }
}
