namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;

public sealed record AssessmentPassRateReportResponse(
    string TargetLanguageCode, string AssessmentType, string ForLevel,
    int TotalAttempts, int PassedCount, int FailedCount,
    decimal PassRate, decimal AverageScore, string Period, DateOnly ReportDate);
