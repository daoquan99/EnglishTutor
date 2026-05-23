using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.AdminReports.Domain.Shared;

namespace EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;

public sealed class LearningActivityReport : Entity<Guid>
{
    public DateOnly ReportDate { get; private set; }
    public ReportPeriod Period { get; private set; }
    public int TotalActiveUsers { get; private set; }
    public int TotalSpeakingSessions { get; private set; }
    public int TotalExercisesCompleted { get; private set; }
    public int TotalVocabularyReviews { get; private set; }
    public int TotalLessonsCompleted { get; private set; }
    public int TotalAssessments { get; private set; }
    public int TotalStudyMinutes { get; private set; }

    private LearningActivityReport() { }

    public static LearningActivityReport Create(
        DateOnly reportDate,
        ReportPeriod period,
        int totalActiveUsers,
        int totalSpeakingSessions,
        int totalExercisesCompleted,
        int totalVocabularyReviews,
        int totalLessonsCompleted,
        int totalAssessments,
        int totalStudyMinutes) =>
        new LearningActivityReport
        {
            Id = Guid.NewGuid(),
            ReportDate = reportDate,
            Period = period
        }.Update(
            totalActiveUsers,
            totalSpeakingSessions,
            totalExercisesCompleted,
            totalVocabularyReviews,
            totalLessonsCompleted,
            totalAssessments,
            totalStudyMinutes);

    public LearningActivityReport Update(
        int totalActiveUsers,
        int totalSpeakingSessions,
        int totalExercisesCompleted,
        int totalVocabularyReviews,
        int totalLessonsCompleted,
        int totalAssessments,
        int totalStudyMinutes)
    {
        TotalActiveUsers = Math.Max(0, totalActiveUsers);
        TotalSpeakingSessions = Math.Max(0, totalSpeakingSessions);
        TotalExercisesCompleted = Math.Max(0, totalExercisesCompleted);
        TotalVocabularyReviews = Math.Max(0, totalVocabularyReviews);
        TotalLessonsCompleted = Math.Max(0, totalLessonsCompleted);
        TotalAssessments = Math.Max(0, totalAssessments);
        TotalStudyMinutes = Math.Max(0, totalStudyMinutes);
        return this;
    }
}
