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
        token.RevokeFamily(DateTime.UtcNow, null);
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

    [Fact]
    public void DetectReuse_Should_Revoke_And_Raise_Event()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), null);
        token.MarkUsed(Guid.NewGuid(), DateTime.UtcNow);
        token.DetectReuse("127.0.0.1");

        token.RevokedAtUtc.Should().NotBeNull();
        token.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "RefreshTokenReuseDetectedDomainEvent");
    }
}
