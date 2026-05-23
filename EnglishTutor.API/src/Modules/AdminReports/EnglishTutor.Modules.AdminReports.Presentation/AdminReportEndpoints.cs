using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.AdminReports.Application.Commands.ReprocessDeadLetter;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;
using EnglishTutor.Modules.Auth.Contracts.Permissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.AdminReports.Presentation;

public static class AdminReportEndpoints
{
    public static IEndpointRouteBuilder MapAdminReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin")
            .RequireAuthorization()
            .WithTags("AdminReports");

        group.MapGet("/reports/users", async (
            int? page,
            int? pageSize,
            string? sortBy,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetUserOverviewReportQuery(page ?? 1, pageSize ?? 20, sortBy), ct)).ToHttpResult();
        });

        group.MapGet("/reports/ai-usage", async (
            DateOnly? from,
            DateOnly? to,
            string? modelType,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAiUsageReportQuery(from, to, modelType), ct)).ToHttpResult();
        });

        group.MapGet("/reports/learning-activity", async (
            DateOnly? from,
            DateOnly? to,
            string? period,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetLearningActivityReportQuery(from, to, period), ct)).ToHttpResult();
        });

        group.MapGet("/reports/mistakes", async (
            string? targetLanguageCode,
            int? top,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetCommonMistakesReportQuery(targetLanguageCode, top ?? 20), ct)).ToHttpResult();
        });

        group.MapGet("/reports/assessments", async (
            DateOnly? from,
            DateOnly? to,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAssessmentPassRatesReportQuery(from, to, targetLanguageCode), ct)).ToHttpResult();
        });

        group.MapGet("/audit-logs", async (
            int? page,
            int? pageSize,
            string? action,
            string? targetEntity,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanReadReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAuditLogsQuery(page ?? 1, pageSize ?? 20, action, targetEntity), ct)).ToHttpResult();
        });

        group.MapGet("/dead-letters", async (
            int? page,
            int? pageSize,
            string? sourceModule,
            string? eventType,
            string? status,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanManageReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetDeadLettersQuery(page ?? 1, pageSize ?? 20, sourceModule, eventType, status), ct)).ToHttpResult();
        });

        group.MapPost("/dead-letters/{id:guid}/reprocess", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!CanManageReports(currentUser))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new ReprocessDeadLetterCommand(id), ct)).ToHttpResult();
        });

        return endpoints;
    }

    private static bool CanReadReports(ICurrentUser currentUser) =>
        currentUser.HasPermission(PermissionCodes.ReportsRead) || currentUser.HasPermission(PermissionCodes.FullAccess);

    private static bool CanManageReports(ICurrentUser currentUser) =>
        currentUser.HasPermission(PermissionCodes.ReportsManage) || currentUser.HasPermission(PermissionCodes.FullAccess);
}
