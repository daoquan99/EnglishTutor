using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using Xunit;

namespace EnglishTutor.Modules.Progress.UnitTests;

public sealed class ProgressDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void GrantExp_Adds_Transaction_And_Updates_Rank()
    {
        var experience = UserExperience.Create(Guid.NewGuid(), LanguageCode.English, UtcNow);

        experience.GrantExp(250, "test", Guid.NewGuid(), "reason", UtcNow);

        Assert.Equal(250, experience.TotalExp);
        Assert.Equal(AppRank.Bronze, experience.CurrentAppRank);
        Assert.Single(experience.Transactions);
    }

    [Fact]
    public void Streak_Continues_On_Consecutive_Days()
    {
        var streak = UserStreak.Create(Guid.NewGuid(), LanguageCode.English);
        var day = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc);

        streak.RecordActivity(day);
        streak.RecordActivity(day.AddDays(1));

        Assert.Equal(2, streak.CurrentStreakDays);
        Assert.Equal(2, streak.LongestStreakDays);
    }

    [Fact]
    public void Streak_Resets_After_Gap()
    {
        var streak = UserStreak.Create(Guid.NewGuid(), LanguageCode.English);
        var day = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc);

        streak.RecordActivity(day);
        streak.RecordActivity(day.AddDays(2));

        Assert.Equal(1, streak.CurrentStreakDays);
        Assert.Equal(1, streak.LongestStreakDays);
    }
}
