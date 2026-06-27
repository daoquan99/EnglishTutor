using EnglishTutor.Audit.Application.Queries.SearchAuditLogs;
using EnglishTutor.Audit.Application.Queries.SearchSecurityEvents;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
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
                PageNumber: pageNumber ?? AuditEndpointDefaults.DefaultPageNumber,
                PageSize: pageSize ?? AuditEndpointDefaults.DefaultPageSize,
                UserId: userId,
                Action: action,
                EntityType: entityType,
                EntityId: entityId,
                StartDate: startDate,
                EndDate: endDate);

            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return ApiResults.FromError(result.Error!);
            }
            var page = result.Value!;
            return ApiResults.Paged(
                items: page.Items,
                page: page.Page,
                pageSize: page.PageSize,
                totalCount: page.TotalCount);
        })
        .RequireAuthorization(policy => policy.RequireClaim(
            AuditEndpointAuthorization.PermissionClaimType,
            AuditEndpointAuthorization.AuditReadPermission));

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
                PageNumber: pageNumber ?? AuditEndpointDefaults.DefaultPageNumber,
                PageSize: pageSize ?? AuditEndpointDefaults.DefaultPageSize,
                UserId: userId,
                CategoryCode: categoryCode,
                StartDate: startDate,
                EndDate: endDate);

            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return ApiResults.FromError(result.Error!);
            }
            var page = result.Value!;
            return ApiResults.Paged(
                items: page.Items,
                page: page.Page,
                pageSize: page.PageSize,
                totalCount: page.TotalCount);
        })
        .RequireAuthorization(policy => policy.RequireClaim(
            AuditEndpointAuthorization.PermissionClaimType,
            AuditEndpointAuthorization.SecurityReadPermission));

        return routes;
    }
}
