using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(int Page, int PageSize, string? Action, string? TargetEntity) : IQuery<IReadOnlyList<AuditLogResponse>>;
