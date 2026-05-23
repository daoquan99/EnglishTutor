namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;

public sealed record LearningActivityReportResponse(
    DateOnly ReportDate, string Period, int TotalActiveUsers,
    int TotalSpeakingSessions, int TotalExercisesCompleted, int TotalVocabularyReviews,
    int TotalLessonsCompleted, int TotalAssessments, int TotalStudyMinutes);
