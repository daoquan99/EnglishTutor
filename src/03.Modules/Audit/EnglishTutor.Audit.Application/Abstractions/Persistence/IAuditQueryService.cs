using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Audit.Application.Queries.SearchAuditLogs;
using EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;

namespace EnglishTutor.Audit.Application.Abstractions.Persistence;

public interface IAuditQueryService
{
    Task<PagedResult<AuditLogResponseDto>> SearchAuditLogsAsync(
        SearchAuditLogsQuery query,
        CancellationToken cancellationToken);

    Task<PagedResult<SecurityEventResponseDto>> SearchSecurityEventsAsync(
        SearchSecurityEventsQuery query,
        CancellationToken cancellationToken);
}
