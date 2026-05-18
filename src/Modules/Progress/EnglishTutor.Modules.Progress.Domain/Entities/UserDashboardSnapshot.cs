using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserDashboardSnapshot : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public DateOnly Date { get; private set; }
    public int TotalExp { get; private set; }
    public string CurrentLevel { get; private set; } = string.Empty;
    public int StreakDays { get; private set; }
    public int VocabularyMastered { get; private set; }
    public int TotalSpeakingSessions { get; private set; }
    public int TotalExercisesCompleted { get; private set; }
    public int TotalMistakes { get; private set; }
    public string WeakSkills { get; private set; } = string.Empty;
    public string StrongSkills { get; private set; } = string.Empty;
    public DateTime LastUpdatedAtUtc { get; private set; }

    private UserDashboardSnapshot() { }

    public static UserDashboardSnapshot Create(Guid userId, LanguageCode targetLanguageCode, DateOnly date)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserDashboardSnapshot
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Date = date,
            CurrentLevel = "A1",
            WeakSkills = string.Empty,
            StrongSkills = string.Empty,
            LastUpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(
        int totalExp,
        string currentLevel,
        int streakDays,
        int vocabularyMastered,
        int totalSpeakingSessions,
        int totalExercisesCompleted,
        int totalMistakes,
        string weakSkills,
        string strongSkills)
    {
        TotalExp = Math.Max(0, totalExp);
        CurrentLevel = LearningActivityLog.Normalize(currentLevel, 10, "Current level");
        StreakDays = Math.Max(0, streakDays);
        VocabularyMastered = Math.Max(0, vocabularyMastered);
        TotalSpeakingSessions = Math.Max(0, totalSpeakingSessions);
        TotalExercisesCompleted = Math.Max(0, totalExercisesCompleted);
        TotalMistakes = Math.Max(0, totalMistakes);
        WeakSkills = string.IsNullOrWhiteSpace(weakSkills) ? string.Empty : weakSkills.Trim();
        StrongSkills = string.IsNullOrWhiteSpace(strongSkills) ? string.Empty : strongSkills.Trim();
        LastUpdatedAtUtc = DateTime.UtcNow;
    }
}
