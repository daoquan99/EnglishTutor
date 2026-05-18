using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetDashboard;

public sealed record GetDashboardQuery(Guid UserId, string TargetLanguageCode) : IQuery<ProgressDashboardResponse>;
