using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Application.Queries.SearchAuditLogs;
using EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Audit.Infrastructure.Persistence;

internal sealed class AuditQueryService : IAuditQueryService
{
    private readonly AuditDbContext _db;

    public AuditQueryService(AuditDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AuditLogResponseDto>> SearchAuditLogsAsync(
        SearchAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = _db.AuditLogs.AsNoTracking();

        // Filters
        if (query.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(l => l.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            dbQuery = dbQuery.Where(l => l.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            dbQuery = dbQuery.Where(l => l.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            dbQuery = dbQuery.Where(l => l.EntityId == query.EntityId);
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(l => l.CreatedAtUtc >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            dbQuery = dbQuery.Where(l => l.CreatedAtUtc <= query.EndDate.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Sorting: created_at_utc DESC, then id DESC
        dbQuery = dbQuery
            .OrderByDescending(l => l.CreatedAtUtc)
            .ThenByDescending(l => l.Id);

        // Pagination
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(1, query.PageNumber);
        var skip = (pageNumber - 1) * pageSize;

        var items = await dbQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(l => new AuditLogResponseDto(
                l.Id,
                l.UserId,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.DetailJson,
                l.IpAddressHash,
                l.UserAgentHash,
                l.CorrelationId,
                l.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<AuditLogResponseDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<SecurityEventResponseDto>> SearchSecurityEventsAsync(
        SearchSecurityEventsQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = _db.SecurityEvents.AsNoTracking();

        // Filters
        if (query.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryCode))
        {
            dbQuery = dbQuery.Where(e => e.CategoryCode == query.CategoryCode);
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.OccurredAtUtc >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.OccurredAtUtc <= query.EndDate.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Sorting: occurred_at_utc DESC, then id DESC
        dbQuery = dbQuery
            .OrderByDescending(e => e.OccurredAtUtc)
            .ThenByDescending(e => e.Id);

        // Pagination
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(1, query.PageNumber);
        var skip = (pageNumber - 1) * pageSize;

        var items = await dbQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(e => new SecurityEventResponseDto(
                e.Id,
                e.CategoryCode,
                e.SourceModule,
                e.SourceEventType,
                e.UserId,
                e.SessionId,
                e.RefreshTokenFamilyId,
                e.RefreshTokenId,
                e.ReasonCode,
                e.CorrelationId,
                e.CausationId,
                e.IpAddressHash,
                e.UserAgentHash,
                e.OccurredAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<SecurityEventResponseDto>.Create(items, totalCount, pageNumber, pageSize);
    }
}
