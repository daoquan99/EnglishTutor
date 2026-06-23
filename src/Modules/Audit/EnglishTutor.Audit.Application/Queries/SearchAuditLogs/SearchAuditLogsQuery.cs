using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Application.Pagination;

namespace EnglishTutor.Audit.Application.Queries.SearchAuditLogs;

public sealed record SearchAuditLogsQuery(
    int PageNumber,
    int PageSize,
    Guid? UserId,
    string? Action,
    string? EntityType,
    string? EntityId,
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<PagedResult<AuditLogResponseDto>>;
