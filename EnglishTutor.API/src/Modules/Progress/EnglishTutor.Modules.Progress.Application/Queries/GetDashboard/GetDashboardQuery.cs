using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetDashboard;

public sealed record GetDashboardQuery(Guid UserId, string TargetLanguageCode) : IQuery<ProgressDashboardResponse>;
