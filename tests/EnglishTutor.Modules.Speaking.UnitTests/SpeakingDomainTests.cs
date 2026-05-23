using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Enums;
using EnglishTutor.Modules.Speaking.Domain.Events;
using EnglishTutor.Modules.Speaking.Domain.ValueObjects;
using Xunit;

namespace EnglishTutor.Modules.Speaking.UnitTests;

public sealed class SpeakingDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Start_Captures_LanguageSnapshot_And_Raises_Event()
    {
        var snapshot = CreateSnapshot();

        var session = SpeakingSession.Start(Guid.NewGuid(), snapshot, SpeakingSessionType.FreeTalk, "travel", UtcNow);

        Assert.Equal(snapshot, session.LanguageSnapshot);
        Assert.Equal(SpeakingSessionStatus.Active, session.Status);
        Assert.Contains(session.DomainEvents, domainEvent => domainEvent is SpeakingSessionStartedDomainEvent);
    }

    [Fact]
    public void AddTurn_Increments_Turn_Number()
    {
        var session = SpeakingSession.Start(Guid.NewGuid(), CreateSnapshot(), SpeakingSessionType.FreeTalk, null, UtcNow);

        var first = session.AddTurn("Hello", UtcNow);
        var second = session.AddTurn("How are you?", UtcNow.AddMinutes(1));

        Assert.Equal(1, first.TurnNumber);
        Assert.Equal(2, second.TurnNumber);
    }

    [Fact]
    public void Completed_Session_Cannot_Add_Turn()
    {
        var session = SpeakingSession.Start(Guid.NewGuid(), CreateSnapshot(), SpeakingSessionType.FreeTalk, null, UtcNow);
        session.Complete(totalTurns: 1, overallScore: 80, UtcNow.AddMinutes(1));

        Assert.Throws<DomainException>(() => session.AddTurn("Again", UtcNow.AddMinutes(2)));
    }

    [Fact]
    public void SpeakingTurnResult_Clamps_Scores_And_Calculates_Overall()
    {
        var result = SpeakingTurnResult.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            LanguageCode.English,
            "I goes",
            "I go",
            "I go",
            110,
            90,
            -5,
            80,
            70,
            "feedback",
            "vi",
            null,
            null,
            null,
            UtcNow);

        Assert.Equal(100, result.GrammarScore);
        Assert.Equal(0, result.PronunciationScore);
        Assert.Equal(68, result.OverallScore);
    }

    [Fact]
    public void SpeakingSessionSummary_Averages_Turn_Results()
    {
        var userId = Guid.NewGuid();
        var results = new[]
        {
            CreateResult(userId, grammar: 80, vocabulary: 90, pronunciation: 70, fluency: 60, task: 100),
            CreateResult(userId, grammar: 100, vocabulary: 80, pronunciation: 90, fluency: 80, task: 90)
        };

        var summary = SpeakingSessionSummary.Create(
            Guid.NewGuid(),
            userId,
            LanguageCode.English,
            results,
            2,
            "Good range",
            "Article usage",
            "Keep practicing");

        Assert.Equal(90, summary.AverageGrammarScore);
        Assert.Equal(85, summary.AverageVocabularyScore);
        Assert.Equal(2, summary.TotalTurns);
    }

    private static LanguageSnapshot CreateSnapshot() =>
        LanguageSnapshot.Create(LanguageCode.Vietnamese, LanguageCode.English, LanguageCode.Vietnamese, LanguageCode.Vietnamese, LanguageLevel.A1);

    private static SpeakingTurnResult CreateResult(Guid userId, int grammar, int vocabulary, int pronunciation, int fluency, int task) =>
        SpeakingTurnResult.Create(
            Guid.NewGuid(),
            userId,
            LanguageCode.English,
            "original",
            "corrected",
            "natural",
            grammar,
            vocabulary,
            pronunciation,
            fluency,
            task,
            "feedback",
            "vi",
            null,
            null,
            null,
            UtcNow);
}
