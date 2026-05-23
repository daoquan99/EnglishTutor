using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Mistakes.Domain.Events;
using Xunit;

namespace EnglishTutor.Modules.Mistakes.UnitTests;

public sealed class MistakeDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateFromCorrection_Sets_New_Status_And_Raises_Event()
    {
        var mistake = CreateMistake();

        Assert.Equal(MistakeStatus.New, mistake.Status);
        Assert.Contains(mistake.DomainEvents, domainEvent => domainEvent is MistakeCreatedDomainEvent);
    }

    [Fact]
    public void Review_Sets_Reviewed_Status_And_Next_Review()
    {
        var mistake = CreateMistake();
        mistake.ClearDomainEvents();

        mistake.Review(UtcNow);

        Assert.Equal(MistakeStatus.Reviewed, mistake.Status);
        Assert.Equal(UtcNow.AddDays(1), mistake.NextReviewAtUtc);
        Assert.Contains(mistake.DomainEvents, domainEvent => domainEvent is MistakeReviewedDomainEvent);
    }

    [Fact]
    public void MarkMastered_Sets_Mastered_Status()
    {
        var mistake = CreateMistake();

        mistake.MarkMastered(UtcNow);

        Assert.Equal(MistakeStatus.Mastered, mistake.Status);
        Assert.NotNull(mistake.MasteredAtUtc);
    }

    private static Mistake CreateMistake() =>
        Mistake.CreateFromCorrection(
            Guid.NewGuid(),
            MistakeSourceType.SpeakingTurn,
            Guid.NewGuid(),
            MistakeType.Grammar,
            "Grammar",
            "I goes",
            "I go",
            "Use base verb after I.",
            LanguageCode.English,
            LanguageCode.Vietnamese,
            LanguageCode.Vietnamese,
            UtcNow);
}
