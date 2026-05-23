using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;

public sealed class UserOverviewCard : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string TargetLanguageCode { get; private set; } = "en";
    public string CurrentLevel { get; private set; } = "A1";
    public int TotalExp { get; private set; }
    public int CurrentStreakDays { get; private set; }
    public int TotalSpeakingSessions { get; private set; }
    public int TotalExercisesCompleted { get; private set; }
    public int TotalVocabularyMastered { get; private set; }
    public int TotalMistakes { get; private set; }
    public DateTime? LastActivityAtUtc { get; private set; }
    public DateTime RegisteredAtUtc { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    private UserOverviewCard() { }

    public static UserOverviewCard Create(Guid userId, string email, string displayName, DateTime registeredAtUtc) =>
        new UserOverviewCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = Normalize(email, 320),
            DisplayName = Normalize(displayName, 200),
            RegisteredAtUtc = registeredAtUtc,
            LastUpdatedAtUtc = registeredAtUtc
        };

    public void UpdateLevel(string targetLanguageCode, string currentLevel, DateTime utcNow)
    {
        TargetLanguageCode = Normalize(targetLanguageCode, 3).ToLowerInvariant();
        CurrentLevel = Normalize(currentLevel, 10);
        LastUpdatedAtUtc = utcNow;
    }

    public void RecordSpeakingSession(DateTime completedAtUtc)
    {
        TotalSpeakingSessions++;
        TouchActivity(completedAtUtc);
    }

    public void RecordExerciseCompleted(DateTime completedAtUtc)
    {
        TotalExercisesCompleted++;
        TouchActivity(completedAtUtc);
    }

    public void RecordVocabularyMastered(DateTime reviewedAtUtc)
    {
        TotalVocabularyMastered++;
        TouchActivity(reviewedAtUtc);
    }

    public void RecordMistake(DateTime createdAtUtc)
    {
        TotalMistakes++;
        TouchActivity(createdAtUtc);
    }

    public void UpdateProgress(int totalExp, int currentStreakDays, DateTime utcNow)
    {
        TotalExp = Math.Max(0, totalExp);
        CurrentStreakDays = Math.Max(0, currentStreakDays);
        LastUpdatedAtUtc = utcNow;
    }

    private void TouchActivity(DateTime utcNow)
    {
        LastActivityAtUtc = LastActivityAtUtc is null || LastActivityAtUtc < utcNow ? utcNow : LastActivityAtUtc;
        LastUpdatedAtUtc = utcNow;
    }

    private static string Normalize(string value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
