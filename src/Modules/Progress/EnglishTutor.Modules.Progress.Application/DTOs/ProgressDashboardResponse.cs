namespace EnglishTutor.Modules.Progress.Application.DTOs;

public sealed record ProgressDashboardResponse(
    Guid UserId,
    string TargetLanguageCode,
    DateOnly Date,
    int TotalExp,
    string CurrentLevel,
    int StreakDays,
    int VocabularyMastered,
    int TotalSpeakingSessions,
    int TotalExercisesCompleted,
    int TotalMistakes,
    string WeakSkills,
    string StrongSkills);
