using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.AdminReports.Domain.Shared;

namespace EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;

public sealed class AssessmentPassRateReport : Entity<Guid>
{
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public string AssessmentType { get; private set; } = string.Empty;
    public string ForLevel { get; private set; } = string.Empty;
    public int TotalAttempts { get; private set; }
    public int PassedCount { get; private set; }
    public int FailedCount { get; private set; }
    public decimal PassRate { get; private set; }
    public decimal AverageScore { get; private set; }
    public ReportPeriod Period { get; private set; }
    public DateOnly ReportDate { get; private set; }

    private AssessmentPassRateReport() { }

    public static AssessmentPassRateReport Create(
        DateOnly reportDate,
        ReportPeriod period,
        string targetLanguageCode,
        string assessmentType,
        string forLevel,
        int totalAttempts,
        int passedCount,
        decimal averageScore) =>
        new AssessmentPassRateReport
        {
            Id = Guid.NewGuid(),
            ReportDate = reportDate,
            Period = period,
            TargetLanguageCode = Normalize(targetLanguageCode, 3).ToLowerInvariant(),
            AssessmentType = Normalize(assessmentType, 50),
            ForLevel = Normalize(forLevel, 10)
        }.Update(totalAttempts, passedCount, averageScore);

    public AssessmentPassRateReport Update(int totalAttempts, int passedCount, decimal averageScore)
    {
        TotalAttempts = Math.Max(0, totalAttempts);
        PassedCount = Math.Clamp(passedCount, 0, TotalAttempts);
        FailedCount = TotalAttempts - PassedCount;
        PassRate = TotalAttempts == 0 ? 0 : Math.Round(PassedCount / (decimal)TotalAttempts, 4, MidpointRounding.AwayFromZero);
        AverageScore = Math.Clamp(Math.Round(averageScore, 2, MidpointRounding.AwayFromZero), 0, 100);
        return this;
    }

    private static string Normalize(string value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
