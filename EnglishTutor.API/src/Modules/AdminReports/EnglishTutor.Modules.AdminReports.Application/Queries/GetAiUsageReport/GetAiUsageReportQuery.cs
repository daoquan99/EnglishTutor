using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;

public sealed record GetAiUsageReportQuery(DateOnly? From, DateOnly? To, string? ModelType) : IQuery<IReadOnlyList<DailyAiUsageReportResponse>>;
