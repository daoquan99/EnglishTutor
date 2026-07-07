using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Entities;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public sealed class LearnerLanguagePortfolioTests
{
    [Fact]
    public void AddPair_FirstPair_ShouldBecomeActive()
    {
        var portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), Guid.NewGuid());
        var pairId = Guid.NewGuid();

        var pair = portfolio.AddPair(pairId, "vi", "en", "vi");

        pair.Status.Should().Be(LearnerLanguagePairStatus.Active);
        portfolio.ActiveLanguagePairId.Should().Be(pairId);
    }

    [Fact]
    public void ActivatePair_ShouldDeactivatePreviousPair()
    {
        var portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), Guid.NewGuid());
        var first = portfolio.AddPair(Guid.NewGuid(), "vi", "en", "vi");
        var second = portfolio.AddPair(Guid.NewGuid(), "vi", "ja", "vi");

        portfolio.ActivatePair(second.Id);

        first.Status.Should().Be(LearnerLanguagePairStatus.Inactive);
        second.Status.Should().Be(LearnerLanguagePairStatus.Active);
        portfolio.ActiveLanguagePairId.Should().Be(second.Id);
    }

    [Fact]
    public void AddPair_WithSameNativeAndTarget_ShouldThrow()
    {
        var portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), Guid.NewGuid());

        var action = () => portfolio.AddPair(Guid.NewGuid(), "en", "en", "en");

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ArchivePair_WhenPairIsActive_ShouldThrow()
    {
        var portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), Guid.NewGuid());
        var pair = portfolio.AddPair(Guid.NewGuid(), "vi", "en", "vi");

        var action = () => portfolio.ArchivePair(pair.Id);

        action.Should().Throw<InvalidOperationException>();
    }
}
