using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;

public sealed record GetLearningActivityReportQuery(DateOnly? From, DateOnly? To, string? Period) : IQuery<IReadOnlyList<LearningActivityReportResponse>>;
