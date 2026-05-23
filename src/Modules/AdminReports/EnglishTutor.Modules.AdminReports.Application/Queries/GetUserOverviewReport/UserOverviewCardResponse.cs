namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;

public sealed record UserOverviewCardResponse(
    Guid UserId, string Email, string DisplayName, string TargetLanguageCode,
    string CurrentLevel, int TotalExp, int CurrentStreakDays,
    int TotalSpeakingSessions, int TotalExercisesCompleted, int TotalVocabularyMastered,
    int TotalMistakes, DateTime? LastActivityAtUtc, DateTime RegisteredAtUtc);
