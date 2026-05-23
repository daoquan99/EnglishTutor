using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Application.Commands.CreateAuditLog;
using MediatR;

namespace EnglishTutor.Api.Middlewares;

public sealed class AdminAuditLogMiddleware(RequestDelegate next, ILogger<AdminAuditLogMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser, ISender sender)
    {
        await next(context);

        if (!ShouldAudit(context, currentUser))
        {
            return;
        }

        try
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var oldValue = JsonSerializer.Serialize(new
            {
                method = context.Request.Method,
                path
            }, JsonOptions);

            var newValue = JsonSerializer.Serialize(new
            {
                statusCode = context.Response.StatusCode,
                traceId = context.TraceIdentifier
            }, JsonOptions);

            await sender.Send(
                new CreateAuditLogCommand(
                    currentUser.UserId,
                    ResolveAction(context),
                    ResolveTargetEntity(path),
                    ResolveTargetEntityId(path),
                    oldValue,
                    newValue,
                    ResolveIpAddress(context),
                    context.Request.Headers.UserAgent.ToString()),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist admin audit log for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
    }

    private static bool ShouldAudit(HttpContext context, ICurrentUser currentUser)
    {
        if (!context.Request.Path.StartsWithSegments("/api/admin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (context.Request.Method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty && context.Response.StatusCode < 500;
    }

    private static string ResolveAction(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.Contains("/dead-letters/", StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith("/reprocess", StringComparison.OrdinalIgnoreCase)
            ? "DeadLetterReprocessed"
            : "AdminCommandExecuted";
    }

    private static string ResolveTargetEntity(string path)
    {
        if (path.Contains("/dead-letters/", StringComparison.OrdinalIgnoreCase))
        {
            return "DeadLetterMessage";
        }

        return "AdminEndpoint";
    }

    private static string ResolveTargetEntityId(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var id = segments.FirstOrDefault(segment => Guid.TryParse(segment, out _));
        return id ?? path;
    }

    // Only trust the connection's remote IP. X-Forwarded-For is attacker-spoofable; if a reverse
    // proxy is involved, configure ForwardedHeadersOptions in Program.cs so ASP.NET rewrites
    // Connection.RemoteIpAddress before this middleware runs.
    private static string? ResolveIpAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
