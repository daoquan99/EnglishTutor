using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;

public sealed record GetCommonMistakesReportQuery(string? TargetLanguageCode, int Top) : IQuery<IReadOnlyList<CommonMistakeStatResponse>>;
