using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;

public sealed class DailyAiUsageReport : Entity<Guid>
{
    public DateOnly ReportDate { get; private set; }
    public string ModelType { get; private set; } = string.Empty;
    public string TaskType { get; private set; } = string.Empty;
    public int TotalRequests { get; private set; }
    public long TotalPromptTokens { get; private set; }
    public long TotalCompletionTokens { get; private set; }
    public long TotalTokens { get; private set; }
    public int AverageLatencyMs { get; private set; }
    public int FailedRequests { get; private set; }
    public decimal EstimatedCostUsd { get; private set; }
}
