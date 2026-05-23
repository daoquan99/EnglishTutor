using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;

public sealed record GetUserOverviewReportQuery(int Page, int PageSize, string? SortBy) : IQuery<IReadOnlyList<UserOverviewCardResponse>>;
