using EnglishTutor.Audit.Application.Queries.SearchAuditLogs;
using EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Audit.Presentation.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder RegisterAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/audit").WithTags("Audit").RequireAuthorization();

        group.MapGet("/logs", async (
            IMediator mediator,
            int? pageNumber,
            int? pageSize,
            Guid? userId,
            string? action,
            string? entityType,
            string? entityId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct) =>
        {
            var query = new SearchAuditLogsQuery(
                PageNumber: pageNumber ?? 1,
                PageSize: pageSize ?? 10,
                UserId: userId,
                Action: action,
                EntityType: entityType,
                EntityId: entityId,
                StartDate: startDate,
                EndDate: endDate);

            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok(result.Value);
        })
        .RequireAuthorization(policy => policy.RequireClaim("permission", "admin.audit_read"));

        group.MapGet("/security-events", async (
            IMediator mediator,
            int? pageNumber,
            int? pageSize,
            Guid? userId,
            string? categoryCode,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct) =>
        {
            var query = new SearchSecurityEventsQuery(
                PageNumber: pageNumber ?? 1,
                PageSize: pageSize ?? 10,
                UserId: userId,
                CategoryCode: categoryCode,
                StartDate: startDate,
                EndDate: endDate);

            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok(result.Value);
        })
        .RequireAuthorization(policy => policy.RequireClaim("permission", "admin.security_read"));

        return routes;
    }
}
