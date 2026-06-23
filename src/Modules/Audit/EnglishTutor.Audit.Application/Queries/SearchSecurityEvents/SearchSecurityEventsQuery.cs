using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Application.Pagination;

namespace EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;

public sealed record SearchSecurityEventsQuery(
    int PageNumber,
    int PageSize,
    Guid? UserId,
    string? CategoryCode,
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<PagedResult<SecurityEventResponseDto>>;
