using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;

public sealed record GetAssessmentPassRatesReportQuery(DateOnly? From, DateOnly? To, string? TargetLanguageCode) : IQuery<IReadOnlyList<AssessmentPassRateReportResponse>>;
