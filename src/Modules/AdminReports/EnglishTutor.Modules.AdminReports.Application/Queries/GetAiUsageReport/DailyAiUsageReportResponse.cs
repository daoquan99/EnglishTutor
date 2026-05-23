namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;

public sealed record DailyAiUsageReportResponse(
    DateOnly ReportDate, string ModelType, string TaskType,
    int TotalRequests, long TotalPromptTokens, long TotalCompletionTokens,
    long TotalTokens, int AverageLatencyMs, int FailedRequests, decimal EstimatedCostUsd);
